using Microsoft.Extensions.Options;
using ThreatStream.Contracts.Events;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Detection.Engine.Abstractions;
using ThreatStream.Detection.Engine.Configuration;
using ThreatStream.Detection.Engine.Models;
using ThreatStream.Detection.Engine.Services;
using ThreatStream.Infrastructure.Configuration;

namespace ThreatStream.Detection.Worker;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMessageConsumer _messageConsumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MessagingOptions _messagingOptions;
    private readonly DetectionEngineOptions _engineOptions;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger,IMessageConsumer messageConsumer, IServiceScopeFactory scopeFactory, MessagingOptions messagingOptions, IOptions<DetectionEngineOptions> engineOptions, IConfiguration configuration)
    {
        _logger = logger;
        _messageConsumer = messageConsumer;
        _scopeFactory = scopeFactory;
        _messagingOptions = messagingOptions;
        _engineOptions = engineOptions.Value;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerGroupId =_configuration["Kafka:ConsumerGroupId"];

        if (string.IsNullOrWhiteSpace(consumerGroupId))
            throw new InvalidOperationException("Kafka:ConsumerGroupId is not configured.");

        if (string.IsNullOrWhiteSpace(_messagingOptions.EnrichedMessagesTopic))
            throw new InvalidOperationException("Kafka:EnrichedMessagesTopic is not configured.");

        if (string.IsNullOrWhiteSpace(_engineOptions.IndexName))
            throw new InvalidOperationException("DetectionEngine:IndexName is not configured.");

        _logger.LogInformation("Detection Worker started. Topic: {Topic}, Group: {Group}", _messagingOptions.EnrichedMessagesTopic, consumerGroupId);

        await _messageConsumer.ConsumeAsync<EnrichedSecurityEventMessage>(_messagingOptions.EnrichedMessagesTopic, consumerGroupId, ProcessEventAsync, stoppingToken);
    }

    private async Task ProcessEventAsync(string? messageKey, EnrichedSecurityEventMessage message)
    {
        if (string.IsNullOrWhiteSpace(messageKey))
            throw new InvalidOperationException("Received an event without a Kafka message key.");

        if (!Guid.TryParse(message.TenantId, out var tenantId))
            throw new InvalidOperationException($"Event '{messageKey}' has an invalid tenant ID.");

        using var scope = _scopeFactory.CreateScope();

        var ruleService = scope.ServiceProvider.GetRequiredService<DetectionRuleService>();

        var queryExecutor = scope.ServiceProvider.GetRequiredService<IDetectionQueryExecutor>();

        var rules = await ruleService.GetActiveRulesAsync(message.TenantId);

        if (rules.Count == 0)
        {
            _logger.LogDebug("No active detection rules for tenant {TenantId}.", message.TenantId);

            return;
        }

        foreach (var rule in rules)
        {
            var matchingEventIds = await ExecuteWithRetryAsync(queryExecutor, rule, CancellationToken.None);

            foreach (var matchedEventId in matchingEventIds)
            {
                var exists = await ruleService.AlertExistsAsync(tenantId, rule.Id, matchedEventId);

                if (exists)
                    continue;

                var alert = new SecurityAlert
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    RuleId = rule.Id,
                    MatchedEventId = matchedEventId,
                    Severity = string.IsNullOrWhiteSpace(message.Severity)? "Medium" : message.Severity,
                    Status = "New",
                    CreatedAt = DateTime.UtcNow
                };

                await ruleService.SaveAlertAsync(alert);

                _logger.LogInformation("Created alert {AlertId} for rule {RuleId}, " + "tenant {TenantId}, matched event {EventId}.",
                    alert.Id,
                    rule.Id,
                    tenantId,
                    matchedEventId);
            }
        }
    }

    private async Task<IReadOnlyList<string>> ExecuteWithRetryAsync(IDetectionQueryExecutor executor, DetectionRule rule, CancellationToken cancellationToken)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= Math.Max(1, _engineOptions.SearchRetryCount); attempt++)
        {
            try
            {
                return await executor.ExecuteAsync(rule, cancellationToken);
            }
            catch (Exception ex) when ( ex is HttpRequestException ||  ex is TaskCanceledException ||  ex is InvalidOperationException)
            {
                lastException = ex;

                _logger.LogWarning(ex, "Detection query failed for rule {RuleId}; attempt {Attempt}.", rule.Id, attempt);

                if (attempt < _engineOptions.SearchRetryCount)
                {
                    await Task.Delay(_engineOptions.SearchRetryDelayMilliseconds, cancellationToken);
                }
            }
        }

        throw new InvalidOperationException( $"Detection rule '{rule.Id}' failed after retries.", lastException);
    }
}
