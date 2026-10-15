using Microsoft.AspNetCore.Mvc;
using ThreatStream.Detection.Engine.Models;
using ThreatStream.Detection.Engine.Services;
using ThreatStream.DetectionManagement.Api.Dtos;

namespace ThreatStream.DetectionManagement.Api.Controllers;

[ApiController]
[Route("api/alerts")]
public sealed class AlertsController : ControllerBase
{
    private const string TenantHeader = "X-Tenant-Id";

    private readonly IAlertManagementService _alertService;

    public AlertsController(IAlertManagementService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAlerts(CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return BadRequest(new
            {
                error = $"A valid {TenantHeader} header is required."
            });
        }

        var alerts = await _alertService.GetAlertsAsync(
            tenantId,
            cancellationToken);

        return Ok(alerts);
    }

    [HttpPatch("{alertId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid alertId, [FromBody] UpdateAlertStatusRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return BadRequest(new
            {
                error = $"A valid {TenantHeader} header is required."
            });
        }

        if (!AlertStatus.IsValid(request.Status))
        {
            return BadRequest(new
            {
                error = "Invalid alert status.",
                allowedStatuses = new[]
                {
                    AlertStatus.New,
                    AlertStatus.InProgress,
                    AlertStatus.Resolved,
                    AlertStatus.FalsePositive
                }
            });
        }

        var updated = await _alertService.UpdateStatusAsync(
            tenantId,
            alertId,
            request.Status,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    private bool TryGetTenantId(out Guid tenantId)
    {
        tenantId = Guid.Empty;

        return Request.Headers.TryGetValue(TenantHeader, out var headerValue)
            && Guid.TryParse(headerValue.ToString(), out tenantId);
    }
}