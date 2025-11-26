using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using mt_backend.Services.Interfaces;
using mt_backend.Models;
using System.Net.Http;
using System.Text.Json;

namespace mt_backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class MicrosoftSsoController : ControllerBase
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IGraphTokenService _graphTokenService;
        private readonly IUserService _userService;
        private readonly ILogger<MicrosoftSsoController> _logger;
        private readonly HttpClient _httpClient;

        public MicrosoftSsoController(
            ITokenAcquisition tokenAcquisition,
            IGraphTokenService graphTokenService,
            IUserService userService,
            ILogger<MicrosoftSsoController> logger,
            HttpClient httpClient)
        {
            _tokenAcquisition = tokenAcquisition;
            _graphTokenService = graphTokenService;
            _userService = userService;
            _logger = logger;
            _httpClient = httpClient;
        }

        // Helper method to get user information from Graph API
        private async Task<object?> GetUserDetailsAsync(string graphToken)
        {
            _logger.LogInformation("GetUserDetailsAsync called with token length: {TokenLength}", graphToken?.Length ?? 0);

            try
            {
                if (string.IsNullOrEmpty(graphToken))
                {
                    _logger.LogWarning("GetUserDetailsAsync: Graph token is null or empty");
                    return null;
                }

                // Call Microsoft Graph /me endpoint to get user information
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {graphToken}");

                _logger.LogInformation("Calling Microsoft Graph API: https://graph.microsoft.com/v1.0/me");
                var response = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");

                _logger.LogInformation("Graph API response status: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Graph API call failed with status: {StatusCode}, Content: {ErrorContent}",
                        response.StatusCode, errorContent);
                    return null;
                }

                var userJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Graph API response received, JSON length: {JsonLength}", userJson?.Length ?? 0);

                var userInfo = JsonSerializer.Deserialize<JsonElement>(userJson);

                // Extract user information
                var userDetails = new
                {
                    id = userInfo.TryGetProperty("id", out var id) ? id.GetString() : null,
                    displayName = userInfo.TryGetProperty("displayName", out var displayName) ? displayName.GetString() : null,
                    userPrincipalName = userInfo.TryGetProperty("userPrincipalName", out var upn) ? upn.GetString() : null,
                    mail = userInfo.TryGetProperty("mail", out var mail) ? mail.GetString() : null,
                    givenName = userInfo.TryGetProperty("givenName", out var givenName) ? givenName.GetString() : null,
                    surname = userInfo.TryGetProperty("surname", out var surname) ? surname.GetString() : null,
                    jobTitle = userInfo.TryGetProperty("jobTitle", out var jobTitle) ? jobTitle.GetString() : null,
                    department = userInfo.TryGetProperty("department", out var department) ? department.GetString() : null
                };

                _logger.LogInformation("User details extracted successfully - ID: {UserId}, DisplayName: {DisplayName}, Email: {Email}",
                    userDetails.id, userDetails.displayName, userDetails.mail ?? userDetails.userPrincipalName);

                return userDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserDetailsAsync failed with exception: {Message}", ex.Message);
                return null;
            }
        }

        // Helper method to save/update user in database
        private async Task<User?> SaveOrUpdateUserAsync(dynamic userDetails)
        {
            try
            {
                if (userDetails?.id == null)
                {
                    _logger.LogWarning("Cannot save user: Azure AD ID is null");
                    return null;
                }

                string azureAdId = userDetails.id;
                string email = userDetails.mail ?? userDetails.userPrincipalName ?? "";
                string displayName = userDetails.displayName ?? "";

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Cannot save user: email is null or empty");
                    return null;
                }

                _logger.LogInformation("Attempting to save/update user - Azure AD ID: {AzureAdId}, Email: {Email}, Name: {DisplayName}",
                    azureAdId, email, displayName);

                // First, try to find user by Azure AD ID
                var existingUser = await _userService.GetUserByAzureAdIdAsync(azureAdId);

                if (existingUser != null)
                {
                    _logger.LogInformation("User found by Azure AD ID, updating: {UserId}", existingUser.Id);

                    // Update existing user's information
                    existingUser.Name = displayName;
                    existingUser.Email = email;

                    await _userService.UpdateUserAsync(existingUser);
                    _logger.LogInformation("User updated successfully: {UserId}", existingUser.Id);
                    return existingUser;
                }

                // If not found by Azure AD ID, try to find by email
                existingUser = await _userService.GetUserByEmailAsync(email);

                if (existingUser != null)
                {
                    _logger.LogInformation("User found by email, linking Azure AD ID: {UserId}", existingUser.Id);

                    // Link existing user with Azure AD ID
                    existingUser.AzureAdId = azureAdId;
                    existingUser.Name = displayName; // Update name as well

                    await _userService.UpdateUserAsync(existingUser);
                    _logger.LogInformation("User linked with Azure AD ID successfully: {UserId}", existingUser.Id);
                    return existingUser;
                }

                // Create new user
                _logger.LogInformation("Creating new user for Azure AD ID: {AzureAdId}", azureAdId);

                var newUser = new User
                {
                    AzureAdId = azureAdId,
                    Name = displayName,
                    Email = email,
                    Password = "", // Teams SSO users don't need a password
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userService.CreateUserAsync(newUser);
                _logger.LogInformation("New user created successfully: {UserId}, Azure AD ID: {AzureAdId}",
                    createdUser.Id, createdUser.AzureAdId);

                return createdUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveOrUpdateUserAsync failed with exception: {Message}", ex.Message);
                return null;
            }
        }

        // New endpoint to get current user information using Graph token
        [HttpGet("user-info")]
        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {
            _logger.LogInformation("GetUserInfo endpoint called");

            try
            {
                _logger.LogInformation("Attempting to acquire Graph token with User.Read scope");

                // Get access token with User.Read scope
                var graphScopes = new[] { "https://graph.microsoft.com/User.Read" };
                var graphToken = await _tokenAcquisition.GetAccessTokenForUserAsync(graphScopes);

                _logger.LogInformation("Token acquisition result: {HasToken}", !string.IsNullOrEmpty(graphToken));

                if (string.IsNullOrEmpty(graphToken))
                {
                    _logger.LogError("Failed to acquire Graph token - token is null or empty");
                    return StatusCode(500, new { error = "Failed to acquire Graph token" });
                }

                var userDetails = await GetUserDetailsAsync(graphToken);

                if (userDetails == null)
                {
                    _logger.LogError("Failed to retrieve user information - userDetails is null");
                    return StatusCode(500, new { error = "Failed to retrieve user information" });
                }

                // Save or update user in database
                var savedUser = await SaveOrUpdateUserAsync(userDetails);

                _logger.LogInformation("GetUserInfo completed successfully");

                return Ok(new
                {
                    success = true,
                    user = userDetails,
                    savedUser = savedUser != null ? new
                    {
                        id = savedUser.Id,
                        name = savedUser.Name,
                        email = savedUser.Email,
                        azureAdId = savedUser.AzureAdId
                    } : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserInfo failed with exception: {Message}", ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Frontend (Teams SSO) should POST to /api/auth/microsoft-sso
        // Authorization: Bearer <teamsSsoToken>
        [HttpPost("microsoft-sso")]
        public async Task<IActionResult> PostMicrosoftSso()
        {
            _logger.LogInformation("PostMicrosoftSso endpoint called");

            try
            {
                _logger.LogInformation("Checking if HttpContext.User is authenticated: {IsAuthenticated}",
                    HttpContext.User?.Identity?.IsAuthenticated == true);

                // If middleware validated the bearer token and populated HttpContext.User, Microsoft.Identity.Web
                // can use GetAccessTokenForUserAsync. But sometimes Teams SSO tokens are raw and middleware may
                // not run for this endpoint depending on registration – we'll support both paths.

                // ✅ UPDATED: Use TeamsActivity.Send for delegated permissions (team-specific)
                var graphScopes = new[] { "https://graph.microsoft.com/TeamsActivity.Send", "https://graph.microsoft.com/User.Read" };
                _logger.LogInformation("Graph scopes defined: {Scopes}", string.Join(", ", graphScopes));

                var userDetails = (object?)null;
                string? token = null;
                User? savedUser = null;

                try
                {
                    // If the request is authenticated by JwtBearer, this will perform OBO using the incoming principal.
                    if (HttpContext.User?.Identity?.IsAuthenticated == true)
                    {
                        _logger.LogInformation("User is authenticated, attempting token acquisition via GetAccessTokenForUserAsync");

                        token = await _tokenAcquisition.GetAccessTokenForUserAsync(graphScopes);
                        _logger.LogInformation("Token acquisition result: {HasToken}, Length: {TokenLength}",
                            !string.IsNullOrEmpty(token), token?.Length ?? 0);

                        if (!string.IsNullOrEmpty(token))
                        {
                            _logger.LogInformation("Getting user details with acquired token");
                            userDetails = await GetUserDetailsAsync(token);

                            if (userDetails != null)
                            {
                                // Save or update user in database
                                savedUser = await SaveOrUpdateUserAsync(userDetails);
                                _logger.LogInformation("User saved/updated in database: {HasSavedUser}", savedUser != null);
                            }

                            _logger.LogInformation("Returning OBO-from-context response with user details: {HasUserDetails}",
                                userDetails != null);

                            return Ok(new
                            {
                                graphToken = token,
                                exchangeType = "obo-from-context",
                                user = userDetails,
                                savedUser = savedUser != null ? new
                                {
                                    id = savedUser.Id,
                                    name = savedUser.Name,
                                    email = savedUser.Email,
                                    azureAdId = savedUser.AzureAdId
                                } : null
                            });
                        }
                        else
                        {
                            _logger.LogWarning("Token acquisition succeeded but returned empty token");
                        }
                    }
                    else
                    {
                        _logger.LogInformation("User is not authenticated via middleware, proceeding to manual OBO");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "GetAccessTokenForUserAsync OBO path failed - will fallback to manual OBO: {Message}", ex.Message);
                }

                // Fallback: read incoming token from Authorization header and do manual OBO via GraphTokenService
                _logger.LogInformation("Starting manual OBO flow");

                var authHeader = Request.Headers["Authorization"].ToString();
                _logger.LogInformation("Authorization header present: {HasHeader}, Starts with Bearer: {StartsWithBearer}",
                    !string.IsNullOrEmpty(authHeader), authHeader.StartsWith("Bearer "));

                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    _logger.LogError("Missing or invalid Authorization header: {Header}", authHeader);
                    return Unauthorized(new { error = "Missing bearer token" });
                }

                var incoming = authHeader.Substring("Bearer ".Length).Trim();
                _logger.LogInformation("Extracted incoming token, length: {TokenLength}", incoming.Length);

                // ✅ UPDATED: Use TeamsActivity.Send for delegated permissions (team-specific)
                var manualScopes = new[] { "https://graph.microsoft.com/TeamsActivity.Send", "https://graph.microsoft.com/User.Read" };
                _logger.LogInformation("Calling GraphTokenService.GetAccessTokenOnBehalfOfAsync with scopes: {Scopes}",
                    string.Join(", ", manualScopes));

                var graphToken = await _graphTokenService.GetAccessTokenOnBehalfOfAsync(manualScopes, incoming);

                _logger.LogInformation("Manual OBO result: {HasToken}, Length: {TokenLength}",
                    !string.IsNullOrEmpty(graphToken), graphToken?.Length ?? 0);

                if (string.IsNullOrEmpty(graphToken))
                {
                    _logger.LogError("Manual OBO returned empty graph token");
                    return StatusCode(500, new { error = "Failed to acquire graph token via OBO" });
                }

                // Get user details using the graph token
                _logger.LogInformation("Getting user details with manual OBO token");
                userDetails = await GetUserDetailsAsync(graphToken);

                if (userDetails != null)
                {
                    // Save or update user in database
                    savedUser = await SaveOrUpdateUserAsync(userDetails);
                    _logger.LogInformation("User saved/updated in database: {HasSavedUser}", savedUser != null);
                }

                _logger.LogInformation("Returning manual-obo response with user details: {HasUserDetails}",
                    userDetails != null);

                return Ok(new
                {
                    graphToken,
                    exchangeType = "manual-obo",
                    user = userDetails,
                    savedUser = savedUser != null ? new
                    {
                        id = savedUser.Id,
                        name = savedUser.Name,
                        email = savedUser.Email,
                        azureAdId = savedUser.AzureAdId
                    } : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostMicrosoftSso failed with exception: {Message}", ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}