using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ThreatStream.Ingestion.Api.Models;
using ThreatStream.Ingestion.Api.Services;

namespace ThreatStream.Ingestion.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase
{
    private const string TenantHeader = "X-Tenant-Id";

    private readonly IEventIngestionService _ingestionService;
    private readonly IValidator<SecurityEventRequest> _validator;

    public EventsController(IEventIngestionService ingestionService, IValidator<SecurityEventRequest> validator)
    {
        _ingestionService = ingestionService;
        _validator = validator;
    }

    [HttpPost("receive")]
    public async Task<IActionResult> ReceiveEvent([FromBody] SecurityEventRequest request, CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue(TenantHeader, out var headerValue) || !Guid.TryParse(headerValue.ToString(), out var tenantId))
        {
            return BadRequest(new{error = $"A valid {TenantHeader} header containing a tenant GUID is required."});
        }

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(
                    error.PropertyName,
                    error.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }
    
        var message = await _ingestionService.IngestAsync(request, tenantId, cancellationToken);

        return Accepted(message);
    }
}
