using System.ComponentModel.DataAnnotations;

namespace ThreatStream.DetectionManagement.Api.Dtos;

public sealed class CreateDetectionRuleRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public string ElasticQueryDsl { get; init; } = string.Empty;
}