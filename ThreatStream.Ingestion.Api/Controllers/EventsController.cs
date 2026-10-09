using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThreatStream.Ingestion.Api.Models;
using ThreatStream.Ingestion.Api.Services;

namespace ThreatStream.Ingestion.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly IEventIngestionService _ingestionService;

        private readonly IValidator<SecurityEventRequest> _validator;

        public EventsController(IEventIngestionService ingestionService, IValidator<SecurityEventRequest> validator)
        {
            _ingestionService = ingestionService;
            _validator = validator;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveEvent([FromBody] SecurityEventRequest request)
        {

            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                return ValidationProblem(ModelState);
            }

            var message = await _ingestionService.IngestAsync(request);

            return Accepted(message);

        }
    }
}

