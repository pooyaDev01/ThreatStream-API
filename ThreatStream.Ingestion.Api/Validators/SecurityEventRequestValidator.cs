using FluentValidation;
using ThreatStream.Ingestion.Api.Models;

namespace ThreatStream.Ingestion.Api.Validators
{
    public class SecurityEventRequestValidator: AbstractValidator<SecurityEventRequest>
    {
        public SecurityEventRequestValidator()
        {
            RuleFor(x => x.Timestamp).NotEmpty().WithMessage("Timestamp is required.");
            RuleFor(x => x.AgentId).NotEmpty().WithMessage("AgentId is required.");
            RuleFor(x => x.EventType).NotEmpty().WithMessage("EventType is required.");
            RuleFor(x => x.ProcessName).NotEmpty().WithMessage("ProcessName is required.");
            RuleFor(x => x.FileHash).NotEmpty().WithMessage("FileHash is required.");
            RuleFor(x => x.User).NotEmpty().WithMessage("User is required.");
            RuleFor(x => x.IpAddress).NotEmpty().WithMessage("IpAddress is required.");
        }
    }
}