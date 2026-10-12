using ThreatStream.Contracts.Events;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Infrastructure.Configuration;

namespace ThreatStream.Detection.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMessageConsumer _messageConsumer;
    private readonly MessagingOptions _messagingOptions;
    private readonly IConfiguration _configuration;

    public Worker(
        ILogger<Worker> logger,
        IMessageConsumer messageConsumer,
        MessagingOptions messagingOptions,
        IConfiguration configuration)
    {
        _logger = logger;
        _messageConsumer = messageConsumer;
        _messagingOptions = messagingOptions;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var consumerGroupId = _configuration["Kafka:ConsumerGroupId"];

        if (string.IsNullOrWhiteSpace(_messagingOptions.EnrichedMessagesTopic))
        {
            throw new InvalidOperationException("Kafka:EnrichedMessagesTopic is not configured.");
        }

        if (string.IsNullOrWhiteSpace(consumerGroupId))
        {
            throw new InvalidOperationException("Kafka:ConsumerGroupId is not configured.");
        }

        _logger.LogInformation("Detection Worker started. Topic: {Topic}, Consumer group: {ConsumerGroup}",
            _messagingOptions.EnrichedMessagesTopic,
            consumerGroupId);

        await _messageConsumer.ConsumeAsync<EnrichedSecurityEventMessage>(
            _messagingOptions.EnrichedMessagesTopic,
            consumerGroupId,
            async (messageKey, message) =>
            {
                _logger.LogInformation("Received enriched event {EventId} for tenant {TenantId}.",
                    messageKey,
                    message.TenantId);

                // Detection rule evaluation will be implemented next.
                await Task.CompletedTask;
            },
            stoppingToken);
    }
}