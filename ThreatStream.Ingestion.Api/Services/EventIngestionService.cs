using Microsoft.Extensions.Options;
using ThreatStream.Contracts.Events;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Infrastructure.Configuration;
using ThreatStream.Ingestion.Api.Models;

namespace ThreatStream.Ingestion.Api.Services;

public class EventIngestionService : IEventIngestionService
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly MessagingOptions _messagingOptions;

    public EventIngestionService(IMessagePublisher messagePublisher, IOptions<MessagingOptions> messagingOptions)
    {
        _messagePublisher = messagePublisher;
        _messagingOptions = messagingOptions.Value;
    }

    public async Task<SecurityEventMessage> IngestAsync(SecurityEventRequest request, Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID must be a non-empty GUID.", nameof(tenantId));
        }

        var message = new SecurityEventMessage(
            tenantId.ToString(),
            request.Timestamp,
            request.AgentId,
            request.EventType,
            request.ProcessName,
            request.FileHash,
            request.User,
            request.IpAddress
        );

        if (string.IsNullOrWhiteSpace(_messagingOptions.SecurityEventsTopic))
        {
            throw new InvalidOperationException("Security events Kafka topic is not configured.");
        }

        var eventId = Guid.NewGuid().ToString("N");

        await _messagePublisher.PublishAsync(message, _messagingOptions.SecurityEventsTopic, eventId);

        return message;
    }
}