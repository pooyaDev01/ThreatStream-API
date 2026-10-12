using System.Text.Json.Serialization;

namespace ThreatStream.Contracts.Events;

public record EnrichedSecurityEventMessage(
    [property: JsonPropertyName("tenant_id")] string TenantId,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp,
    [property: JsonPropertyName("agent_id")] string AgentId,
    [property: JsonPropertyName("event_type")] string EventType,
    [property: JsonPropertyName("process_name")] string ProcessName,
    [property: JsonPropertyName("file_hash")] string FileHash,
    [property: JsonPropertyName("user")] string User,
    [property: JsonPropertyName("ip_address")] string IpAddress,
    [property: JsonPropertyName("is_malicious")] string IsMalicious,
    [property: JsonPropertyName("threat_name")] string? ThreatName,
    [property: JsonPropertyName("severity")] string? Severity
);