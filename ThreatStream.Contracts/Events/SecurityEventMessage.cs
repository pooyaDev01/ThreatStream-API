namespace ThreatStream.Contracts.Events
{
    public record SecurityEventMessage
    (
        string TenantId,
        DateTime Timestamp,
        string AgentId,
        string EventType,
        string ProcessName,
        string FileHash,
        string User,
        string IpAddress
    );
}
