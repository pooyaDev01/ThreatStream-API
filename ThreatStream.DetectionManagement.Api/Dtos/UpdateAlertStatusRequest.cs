using System.ComponentModel.DataAnnotations;

namespace ThreatStream.DetectionManagement.Api.Dtos;

public sealed class UpdateAlertStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
