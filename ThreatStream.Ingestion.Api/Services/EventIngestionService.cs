using ThreatStream.Contracts.Events;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Ingestion.Api.Configuration;
using ThreatStream.Ingestion.Api.Models;

namespace ThreatStream.Ingestion.Api.Services
{
    public class EventIngestionService : IEventIngestionService
    {
        private readonly ITenantContext _tenantContext;
        private readonly IMessagePublisher _messagePublisher;
        private readonly MessagingOptions _messagingOptions;
        public EventIngestionService(ITenantContext tenantContext, IMessagePublisher messagePublisher, MessagingOptions messagingOptions)
        {
            _tenantContext = tenantContext;
            _messagePublisher = messagePublisher;
            _messagingOptions = messagingOptions;
        }

        public async Task<SecurityEventMessage> IngestAsync(SecurityEventRequest request)
        {
            var tenantId = _tenantContext.GetTenantId();

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new UnauthorizedAccessException("Tenant ID could not be determined.");
            }

            var message = new SecurityEventMessage
            (
                tenantId,
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
                throw new InvalidOperationException(
                    "Security events Kafka topic is not configured.");
            }

            await _messagePublisher.PublishAsync(message, _messagingOptions.SecurityEventsTopic);

            return message;
        }
    }
}
