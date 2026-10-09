namespace ThreatStream.Enrichment.Worker.Models;

public class ThreatIntelData
{
    public string IsMalicious { get; set; } = "unknown";

    public string? ThreatName { get; set; }

    public string? Severity { get; set; }
}