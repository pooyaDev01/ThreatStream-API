using ThreatStream.Contracts.Events;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Ingestion.Api.Models;

namespace ThreatStream.Ingestion.Api.Services
{
    public class EventIngestionService : IEventIngestionService
    {
        private readonly ITenantContext _tenantContext;
        private readonly IMessagePublisher _messagePublisher;

        public EventIngestionService(ITenantContext tenantContext, IMessagePublisher messagePublisher)
        {
            _tenantContext = tenantContext;
            _messagePublisher = messagePublisher;
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

            await _messagePublisher.PublishAsync( message, "security-events");

            return message;
        }
    }
}
