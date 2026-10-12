
namespace ThreatStream.Detection.Engine.Models;

public class DetectionRule
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ElasticQueryDsl { get; set; } = "{}";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}
