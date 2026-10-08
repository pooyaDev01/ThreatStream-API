using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThreatStream.Ingestion.Api.Models;
using ThreatStream.Ingestion.Api.Services;

namespace ThreatStream.Ingestion.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventIngestionService _ingestionService;

        public EventsController(IEventIngestionService ingestionService)
        {
            _ingestionService = ingestionService;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveEvent([FromBody] SecurityEventRequest request)
        {
            var message = await _ingestionService.IngestAsync(request);

            return Accepted(message);

        }
    }
}

