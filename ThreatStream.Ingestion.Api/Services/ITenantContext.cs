namespace ThreatStream.Ingestion.Api.Services
{
    public interface ITenantContext
    {
        string? GetTenantId();
    }
}
