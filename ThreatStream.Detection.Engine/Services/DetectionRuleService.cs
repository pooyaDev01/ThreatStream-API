
using Microsoft.EntityFrameworkCore;
using ThreatStream.Detection.Engine.Models;

namespace ThreatStream.Detection.Engine.Services;

public sealed class DetectionRuleService
{
    private readonly DetectionDbContext _dbContext;

    public DetectionRuleService(DetectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DetectionRule>> GetActiveRulesAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(tenantId, out var parsedTenantId))
        {
            throw new ArgumentException( "Tenant ID must be a valid GUID.", nameof(tenantId));
        }

        return await _dbContext.DetectionRules
            .AsNoTracking()
            .Where(rule =>
                rule.IsActive &&
                rule.TenantId == parsedTenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AlertExistsAsync(Guid tenantId, Guid ruleId, string matchedEventId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SecurityAlerts
            .AsNoTracking()
            .AnyAsync(alert =>
                alert.TenantId == tenantId &&
                alert.RuleId == ruleId &&
                alert.MatchedEventId == matchedEventId,
                cancellationToken);
    }

    public async Task SaveAlertAsync(SecurityAlert alert, CancellationToken cancellationToken = default)
    {
        _dbContext.SecurityAlerts.Add(alert);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            _dbContext.Entry(alert).State = EntityState.Detached;

            var exists = await AlertExistsAsync(alert.TenantId, alert.RuleId, alert.MatchedEventId, cancellationToken);

            if (!exists)
                throw;
        }
    }
}
