using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ThreatStream.Detection.Engine.Models;
using ThreatStream.Detection.Engine.Services;
using ThreatStream.DetectionManagement.Api.Dtos;

namespace ThreatStream.DetectionManagement.Api.Controllers;

[ApiController]
[Route("api/detection-rules")]
public sealed class DetectionRulesController : ControllerBase
{
    private const string TenantHeader = "X-Tenant-Id";

    private readonly DetectionRuleService _ruleService;

    public DetectionRulesController(DetectionRuleService ruleService)
    {
        _ruleService = ruleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRules(CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
            return BadRequest(new { error = $"A valid {TenantHeader} header is required." });

        var rules = await _ruleService.GetRulesAsync(tenantId, cancellationToken);

        return Ok(rules);
    }

    [HttpGet("{ruleId:guid}")]
    public async Task<IActionResult> GetRule(Guid ruleId, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
            return BadRequest(new { error = $"A valid {TenantHeader} header is required." });

        var rule = await _ruleService.GetRuleAsync(tenantId, ruleId, cancellationToken);

        return rule is null ? NotFound() : Ok(rule);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRule([FromBody] CreateDetectionRuleRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
            return BadRequest(new { error = $"A valid {TenantHeader} header is required." });

        if (!IsValidElasticQuery(request.ElasticQueryDsl))
        {
            return BadRequest(new {error = "ElasticQueryDsl must be valid JSON containing a top-level 'query' property."});
        }

        var rule = new DetectionRule
        {
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Description = request.Description,
            ElasticQueryDsl = request.ElasticQueryDsl,
            IsActive = false
        };

        var createdRule = await _ruleService.CreateRuleAsync(rule, cancellationToken);

        return CreatedAtAction(nameof(GetRule), new { ruleId = createdRule.Id }, createdRule);
    }

    [HttpPut("{ruleId:guid}")]
    public async Task<IActionResult> UpdateRule(Guid ruleId, [FromBody] UpdateDetectionRuleRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
            return BadRequest(new { error = $"A valid {TenantHeader} header is required." });

        if (!IsValidElasticQuery(request.ElasticQueryDsl))
        {
            return BadRequest(new { error = "ElasticQueryDsl must be valid JSON containing a top-level 'query' property." });
        }

        var updated = await _ruleService.UpdateRuleAsync(tenantId, ruleId, request.Name.Trim(), request.Description, request.ElasticQueryDsl, cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPatch("{ruleId:guid}/active")]
    public async Task<IActionResult> SetRuleActive(Guid ruleId, [FromQuery] bool isActive, CancellationToken cancellationToken)
    {
        if (!TryGetTenantId(out var tenantId))
            return BadRequest(new { error = $"A valid {TenantHeader} header is required." });

        var updated = await _ruleService.SetRuleActiveAsync(tenantId, ruleId, isActive, cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    private bool TryGetTenantId(out Guid tenantId)
    {
        tenantId = Guid.Empty;

        return Request.Headers.TryGetValue(TenantHeader, out var headerValue) && Guid.TryParse(headerValue.ToString(), out tenantId);
    }

    private static bool IsValidElasticQuery(string dsl)
    {
        if (string.IsNullOrWhiteSpace(dsl))
            return false;

        try
        {
            using var document = JsonDocument.Parse(dsl);

            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("query", out var query)
                && query.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
