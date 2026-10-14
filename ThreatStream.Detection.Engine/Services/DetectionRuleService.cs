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
        var parsedTenantId = ParseTenantId(tenantId);

        return await _dbContext.DetectionRules
            .AsNoTracking()
            .Where(rule => rule.IsActive && rule.TenantId == parsedTenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DetectionRule>> GetRulesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DetectionRules
            .AsNoTracking()
            .Where(rule => rule.TenantId == tenantId)
            .OrderByDescending(rule => rule.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<DetectionRule?> GetRuleAsync(Guid tenantId, Guid ruleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DetectionRules
            .AsNoTracking()
            .FirstOrDefaultAsync(rule => rule.TenantId == tenantId && rule.Id == ruleId, cancellationToken);
    }

    public async Task<DetectionRule> CreateRuleAsync(DetectionRule rule, CancellationToken cancellationToken = default)
    {
        rule.Id = Guid.NewGuid();
        rule.CreatedAt = DateTime.UtcNow;

        _dbContext.DetectionRules.Add(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return rule;
    }

    public async Task<bool> UpdateRuleAsync(Guid tenantId, Guid ruleId, string name, string description, string elasticQueryDsl, CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.DetectionRules
            .FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Id == ruleId, cancellationToken);

        if (rule is null)
            return false;

        rule.Name = name;
        rule.Description = description;
        rule.ElasticQueryDsl = elasticQueryDsl;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetRuleActiveAsync(Guid tenantId, Guid ruleId, bool isActive, CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.DetectionRules
            .FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Id == ruleId, cancellationToken);

        if (rule is null)
            return false;

        rule.IsActive = isActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AlertExistsAsync(Guid tenantId, Guid ruleId, string matchedEventId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SecurityAlerts
            .AsNoTracking()
            .AnyAsync(alert => alert.TenantId == tenantId
                     && alert.RuleId == ruleId
                     && alert.MatchedEventId == matchedEventId,
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

    private static Guid ParseTenantId(string tenantId)
    {
        if (!Guid.TryParse(tenantId, out var parsedTenantId))
        {
            throw new ArgumentException("Tenant ID must be a valid GUID.",nameof(tenantId));
        }

        return parsedTenantId;
    }
}
