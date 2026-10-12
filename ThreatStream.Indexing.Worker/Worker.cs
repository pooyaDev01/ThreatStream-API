using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using ThreatStream.Contracts.Events;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Indexing.Worker.Configuration;
using ThreatStream.Indexing.Worker.Setup;
using ThreatStream.Infrastructure.Configuration;

namespace ThreatStream.Indexing.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMessageConsumer _messageConsumer;
    private readonly ElasticsearchClient _elasticsearchClient;
    private readonly ElasticsearchOptions _elasticsearchOptions;
    private readonly IConfiguration _configuration;
    private readonly MessagingOptions _messagingOptions;

    public Worker(ILogger<Worker> logger, IMessageConsumer messageConsumer, MessagingOptions messagingOptions,  ElasticsearchClient elasticsearchClient, IOptions<ElasticsearchOptions> elasticsearchOptions, IConfiguration configuration)
    {
        _logger = logger;
        _messageConsumer = messageConsumer;
        _elasticsearchClient = elasticsearchClient;
        _elasticsearchOptions = elasticsearchOptions.Value;
        _messagingOptions = messagingOptions;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerGroupId = _configuration["Kafka:ConsumerGroupId"];

        if (string.IsNullOrWhiteSpace(consumerGroupId))
        {
            throw new InvalidOperationException("Kafka:ConsumerGroupId is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_elasticsearchOptions.IndexName))
        {
            throw new InvalidOperationException("Elasticsearch:IndexName is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_elasticsearchOptions.Url))
        {
            throw new InvalidOperationException("Elasticsearch:IndexName is not configured.");
        }

        _logger.LogInformation(
            "Indexing Worker started. Topic: {Topic}, Consumer group: {ConsumerGroup}",
            _messagingOptions.EnrichedMessagesTopic,
            consumerGroupId);

        await ElasticsearchIndexSetup.EnsureIndexAsync(_elasticsearchClient, _elasticsearchOptions.IndexName, stoppingToken);

        await _messageConsumer.ConsumeAsync<EnrichedSecurityEventMessage>(_messagingOptions.EnrichedMessagesTopic,
            consumerGroupId,
            async (messageKey, message) =>
            {
                if (string.IsNullOrWhiteSpace(messageKey))
                {
                    throw new InvalidOperationException(
                        "Received an enriched event without a Kafka message key.");
                }

                var response = await _elasticsearchClient.IndexAsync(message, request =>
                {
                    request.Index(_elasticsearchOptions.IndexName)
                    .Id(messageKey);

                }, stoppingToken);

                if (!response.IsValidResponse)
                {
                    throw new InvalidOperationException($"Failed to index event {messageKey} in Elasticsearch: " + $"{response.DebugInformation}");
                }

                _logger.LogInformation("Indexed event {EventId} for tenant {TenantId}.",
                    messageKey,
                    message.TenantId);

            }, stoppingToken);

    }
}
