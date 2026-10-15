using Microsoft.EntityFrameworkCore;
using ThreatStream.Detection.Engine.Models;

namespace ThreatStream.Detection.Engine.Services;

public sealed class AlertManagementService : IAlertManagementService
{
    private readonly DetectionDbContext _dbContext;

    public AlertManagementService(DetectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SecurityAlert>> GetAlertsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SecurityAlerts
            .AsNoTracking()
            .Where(alert => alert.TenantId == tenantId)
            .OrderByDescending(alert => alert.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateStatusAsync(Guid tenantId, Guid alertId, string status, CancellationToken cancellationToken = default)
    {
        if (!AlertStatus.IsValid(status))
        {
            throw new ArgumentException($"Invalid alert status: {status}", nameof(status));
        }

        var alert = await _dbContext.SecurityAlerts
            .FirstOrDefaultAsync(alert => alert.TenantId == tenantId && alert.Id == alertId, cancellationToken);

        if (alert is null)
        {
            return false;
        }

        alert.Status = status;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}