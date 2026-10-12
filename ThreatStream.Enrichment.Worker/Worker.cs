using ThreatStream.Contracts.Events;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Enrichment.Worker.Services;
using ThreatStream.Infrastructure.Configuration;

namespace ThreatStream.Enrichment.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMessageConsumer _messageConsumer;
    private readonly IMessagePublisher _messagePublisher;
    private readonly RedisThreatIntelService _threatIntelService;
    private readonly MessagingOptions _messagingOptions;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger,
        IMessageConsumer messageConsumer,
        IMessagePublisher messagePublisher,
        RedisThreatIntelService threatIntelService,
        MessagingOptions messagingOptions,
        IConfiguration configuration)
    {
        _logger = logger;
        _messageConsumer = messageConsumer;
        _messagePublisher = messagePublisher;
        _threatIntelService = threatIntelService;
        _messagingOptions = messagingOptions;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerGroupId = _configuration["Kafka:ConsumerGroupId"];

        if (string.IsNullOrWhiteSpace(_messagingOptions.SecurityEventsTopic))
            throw new InvalidOperationException("Kafka:SecurityEventsTopic is not configured.");

        if (string.IsNullOrWhiteSpace(_messagingOptions.EnrichedMessagesTopic))
            throw new InvalidOperationException("Kafka:EnrichedMessagesTopic is not configured.");

        if (string.IsNullOrWhiteSpace(consumerGroupId))
            throw new InvalidOperationException("Kafka:ConsumerGroupId is not configured.");

        _logger.LogInformation("Enrichment Worker started.");

        await _messageConsumer.ConsumeAsync<SecurityEventMessage>(_messagingOptions.SecurityEventsTopic, consumerGroupId,
        async (messageKey, message) =>
        {
            var threatIntel = await _threatIntelService.GetByHashAsync(message.FileHash, stoppingToken);

            var enrichedMessage = new EnrichedSecurityEventMessage(
                message.TenantId,
                message.Timestamp,
                message.AgentId,
                message.EventType,
                message.ProcessName,
                message.FileHash,
                message.User,
                message.IpAddress,
                threatIntel?.IsMalicious ?? "unknown",
                threatIntel?.ThreatName,
                threatIntel?.Severity);

            await _messagePublisher.PublishAsync(enrichedMessage, _messagingOptions.EnrichedMessagesTopic, messageKey);

            _logger.LogInformation("Enriched event published for agent {AgentId}.", message.AgentId);

        }, stoppingToken);
        
    }    
    
}