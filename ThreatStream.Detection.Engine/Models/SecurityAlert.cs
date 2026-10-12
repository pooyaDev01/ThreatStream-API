
namespace ThreatStream.Detection.Engine.Models;

public class SecurityAlert
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RuleId { get; set; }

    public string MatchedEventId { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public string Status { get; set; } = "New";

    public DateTime CreatedAt { get; set; }
}
