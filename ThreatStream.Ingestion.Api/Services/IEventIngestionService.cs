using ThreatStream.Contracts.Events;
using ThreatStream.Ingestion.Api.Models;

namespace ThreatStream.Ingestion.Api.Services
{
    public interface IEventIngestionService
    {
        Task<SecurityEventMessage> IngestAsync(SecurityEventRequest request);
    }
}
