using ThreatStream.Detection.Engine.Models;

namespace ThreatStream.Detection.Engine.Services;

public interface IAlertManagementService
{
    Task<IReadOnlyList<SecurityAlert>> GetAlertsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<bool> UpdateStatusAsync(Guid tenantId, Guid alertId, string status, CancellationToken cancellationToken = default);
}
