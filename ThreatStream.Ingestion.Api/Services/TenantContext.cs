using System.IdentityModel.Tokens.Jwt;

namespace ThreatStream.Ingestion.Api.Services
{
    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetTenantId()
        {
            var authorizationHeader = _httpContextAccessor
                .HttpContext?
                .Request
                .Headers
                .Authorization
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authorizationHeader))
                return null;

            if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return null;

            var token = authorizationHeader["Bearer ".Length..].Trim();

            if (string.IsNullOrWhiteSpace(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            return jwt.Claims
                .FirstOrDefault(claim => claim.Type == "tenantId")
                ?.Value;
        }
    }
}
