using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using mt_backend.Models;
using mt_backend.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace mt_backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class MicrosoftAuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IErrorLogger _errorLogger;
        private readonly IConfiguration _configuration;

        public MicrosoftAuthController(IUserService userService, IErrorLogger errorLogger, IConfiguration configuration)
        {
            _userService = userService;
            _errorLogger = errorLogger;
            _configuration = configuration;
        }

        [HttpPost("microsoft")]
        public async Task<IActionResult> MicrosoftLogin()
        {
            try
            {
                await _errorLogger.LogAsync(
                    "Microsoft login attempt",
                    "Processing Microsoft authentication request",
                    "MicrosoftAuthController.MicrosoftLogin"
                );

                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    await _errorLogger.LogAsync(
                        "Microsoft login failed - missing token",
                        $"Auth header: {authHeader}",
                        "MicrosoftAuthController.MicrosoftLogin"
                    );
                    return Unauthorized("Missing or invalid token");
                }

                var token = authHeader.Substring("Bearer ".Length);

                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken;

                try
                {
                    jwtToken = handler.ReadJwtToken(token);
                }
                catch (Exception ex)
                {
                    await _errorLogger.LogAsync(
                        "Microsoft login failed - token parsing error",
                        $"Error: {ex.Message}, Token length: {token.Length}",
                        "MicrosoftAuthController.MicrosoftLogin"
                    );
                    return Unauthorized("Invalid token format");
                }

                // ✅ ENHANCED: Better claims extraction for Teams tokens
                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ??
                           jwtToken.Claims.FirstOrDefault(c => c.Type == "upn")?.Value ??
                           jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

                var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value ??
                          jwtToken.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;

                var azureAdId = jwtToken.Claims.FirstOrDefault(c => c.Type == "oid")?.Value ??
                              jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                // ✅ ENHANCED: Log token details for debugging
                await _errorLogger.LogAsync(
                    "Token claims extracted",
                    $"Email: {email}, Name: {name}, AzureAdId: {azureAdId}, " +
                    $"Audience: {jwtToken.Audiences?.FirstOrDefault()}, " +
                    $"Issuer: {jwtToken.Issuer}",
                    "MicrosoftAuthController.MicrosoftLogin"
                );

                if (string.IsNullOrEmpty(email))
                {
                    await _errorLogger.LogAsync(
                        "Microsoft login failed - email not found",
                        $"Available claims: {string.Join(", ", jwtToken.Claims.Select(c => $"{c.Type}={c.Value}"))}",
                        "MicrosoftAuthController.MicrosoftLogin"
                    );
                    return Unauthorized("Email not found in token");
                }

                if (string.IsNullOrEmpty(azureAdId))
                {
                    await _errorLogger.LogAsync(
                        "Microsoft login failed - Azure AD ID not found",
                        $"Available claims: {string.Join(", ", jwtToken.Claims.Select(c => $"{c.Type}={c.Value}"))}",
                        "MicrosoftAuthController.MicrosoftLogin"
                    );
                    return Unauthorized("Azure AD ID not found in token");
                }

                // First, try to find user by Azure AD ID
                var user = await _userService.GetUserByAzureAdIdAsync(azureAdId);

                if (user == null)
                {
                    // If not found by Azure AD ID, try to find by email
                    user = await _userService.GetUserByEmailAsync(email);

                    if (user != null)
                    {
                        // ✅ IMPORTANT: Update existing user with Azure AD ID
                        await _errorLogger.LogAsync(
                            "Updating existing user with Azure AD ID",
                            $"User: {user.Email}, Azure AD ID: {azureAdId}",
                            "MicrosoftAuthController.MicrosoftLogin"
                        );

                        user.AzureAdId = azureAdId;

                        // Also update name if it's different/better
                        if (!string.IsNullOrEmpty(name) && name != user.Name)
                        {
                            user.Name = name;
                        }

                        await _userService.UpdateUserAsync(user);
                    }
                    else
                    {
                        // Create new user if doesn't exist
                        await _errorLogger.LogAsync(
                            "Creating new user from Microsoft login",
                            $"Email: {email}, Azure AD ID: {azureAdId}",
                            "MicrosoftAuthController.MicrosoftLogin"
                        );

                        user = new User
                        {
                            Name = name ?? email,
                            Email = email,
                            AzureAdId = azureAdId
                        };
                        user = await _userService.CreateUserAsync(user);
                    }
                }
                else
                {
                    await _errorLogger.LogAsync(
                        "Found existing user by Azure AD ID",
                        $"User: {user.Email}, Azure AD ID: {azureAdId}",
                        "MicrosoftAuthController.MicrosoftLogin"
                    );
                }

                await _errorLogger.LogAsync(
                    "Microsoft login successful",
                    $"User ID: {user.Id}, Email: {user.Email}",
                    "MicrosoftAuthController.MicrosoftLogin"
                );

                var response = new
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    AzureAdId = user.AzureAdId  // ✅ Include AzureAdId in response for debugging
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    "Microsoft login failed with exception",
                    $"Error: {ex.Message}\nStackTrace: {ex.StackTrace}",
                    "MicrosoftAuthController.MicrosoftLogin"
                );

                return StatusCode(500, new { error = "Internal server error during Microsoft login" });
            }
        }

    }
}