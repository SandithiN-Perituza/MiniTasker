using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using mt_backend.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace mt_backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class MicrosoftSsoController : ControllerBase
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IGraphTokenService _graphTokenService;
        private readonly ILogger<MicrosoftSsoController> _logger;

        public MicrosoftSsoController(
            ITokenAcquisition tokenAcquisition,
            IGraphTokenService graphTokenService,
            ILogger<MicrosoftSsoController> logger)
        {
            _tokenAcquisition = tokenAcquisition;
            _graphTokenService = graphTokenService;
            _logger = logger;
        }

        // Frontend (Teams SSO) should POST to /api/auth/microsoft-sso
        // Authorization: Bearer <teamsSsoToken>
        [HttpPost("microsoft-sso")]
        public async Task<IActionResult> PostMicrosoftSso()
        {
            try
            {
                // If middleware validated the bearer token and populated HttpContext.User, Microsoft.Identity.Web
                // can use GetAccessTokenForUserAsync. But sometimes Teams SSO tokens are raw and middleware may
                // not run for this endpoint depending on registration – we'll support both paths.

                // ✅ UPDATED: Use TeamsActivity.Send for delegated permissions (team-specific)
                var graphScopes = new[] { "https://graph.microsoft.com/TeamsActivity.Send" };

                try
                {
                    // If the request is authenticated by JwtBearer, this will perform OBO using the incoming principal.
                    if (HttpContext.User?.Identity?.IsAuthenticated == true)
                    {
                        var token = await _tokenAcquisition.GetAccessTokenForUserAsync(graphScopes);
                        if (!string.IsNullOrEmpty(token))
                        {
                            return Ok(new { graphToken = token, exchangeType = "obo-from-context" });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "GetAccessTokenForUserAsync OBO path failed - will fallback to manual OBO");
                }

                // Fallback: read incoming token from Authorization header and do manual OBO via GraphTokenService
                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                    return Unauthorized(new { error = "Missing bearer token" });

                var incoming = authHeader.Substring("Bearer ".Length).Trim();

                // ✅ UPDATED: Use TeamsActivity.Send for delegated permissions (team-specific)
                var manualScopes = new[] { "https://graph.microsoft.com/TeamsActivity.Send" };
                var graphToken = await _graphTokenService.GetAccessTokenOnBehalfOfAsync(manualScopes, incoming);

                if (string.IsNullOrEmpty(graphToken))
                {
                    _logger.LogWarning("Manual OBO returned empty graph token");
                    return StatusCode(500, new { error = "Failed to acquire graph token via OBO" });
                }

                return Ok(new { graphToken, exchangeType = "manual-obo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "microsoft-sso exchange failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}