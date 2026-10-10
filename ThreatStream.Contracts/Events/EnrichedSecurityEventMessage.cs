using System;
using System.Collections.Generic;
using System.Text;

namespace ThreatStream.Contracts.Events;

public record EnrichedSecurityEventMessage(
    string TenantId,
    DateTime Timestamp,
    string AgentId,
    string EventType,
    string ProcessName,
    string FileHash,
    string User,
    string IpAddress,
    string IsMalicious,
    string? ThreatName,
    string? Severity
);
