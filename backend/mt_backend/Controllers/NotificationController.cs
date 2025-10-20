using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using mt_backend.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using mt_backend.Data;
using Microsoft.EntityFrameworkCore;

namespace mt_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly IErrorLogger _errorLogger;
        private readonly IConfiguration _configuration;
        private readonly MiniTaskerDbContext _context;

        public NotificationController(
            INotificationService notificationService,
            IUserService userService,
            IErrorLogger errorLogger,
            IConfiguration configuration,
            MiniTaskerDbContext context)
        {
            _notificationService = notificationService;
            _userService = userService;
            _errorLogger = errorLogger;
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// Get recent error logs from database
        /// </summary>
        [HttpGet("error-logs")]
        [AllowAnonymous]
        public async Task<IActionResult> GetErrorLogs()
        {
            try
            {
                var errorLogs = await _context.ErrorLogs
                    .OrderByDescending(e => e.Timestamp)
                    .Take(20)
                    .Select(e => new
                    {
                        e.Id,
                        e.Timestamp,
                        e.Message,
                        e.Source,
                        StackTraceSnippet = e.StackTrace.Length > 100 ? e.StackTrace.Substring(0, 100) + "..." : e.StackTrace
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    count = errorLogs.Count,
                    errorLogs = errorLogs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to retrieve error logs",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Test endpoint to verify error logging is working - NO AUTH REQUIRED
        /// </summary>
        [HttpGet("test-logging")]
        [AllowAnonymous]
        public async Task<IActionResult> TestLogging()
        {
            try
            {
                await _errorLogger.LogAsync(
                    "Test log entry from NotificationController",
                    "This is a test to verify error logging is working",
                    "NotificationController.TestLogging"
                );

                return Ok(new
                {
                    success = true,
                    message = "Test log entry created successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to create test log entry",
                    details = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Test endpoint to force an exception and see if it gets logged
        /// </summary>
        [HttpGet("test-exception")]
        [AllowAnonymous]
        public async Task<IActionResult> TestException()
        {
            await _errorLogger.LogAsync(
                "About to throw test exception",
                "Testing exception handling",
                "NotificationController.TestException"
            );

            // Force an exception
            throw new Exception("This is a test exception to verify error logging");
        }

        /// <summary>
        /// Send a test notification to the currently logged-in user
        /// This will be triggered by a button click from the frontend
        /// </summary>
        [HttpPost("send-test")]
        [Authorize] // Require authentication for notification endpoints
        public async Task<IActionResult> SendTestNotification()
        {
            try
            {
                // Log the start of the method
                await _errorLogger.LogAsync(
                    "SendTestNotification endpoint called",
                    "Method execution started",
                    "NotificationController.SendTestNotification"
                );

                // Get configuration for both frontend and backend
                var tenantId = _configuration["AzureAd:TenantId"] ?? "7b967b11-c0b9-402b-b483-d694f50dfb82";
                var backendClientId = _configuration["AzureAd:ClientId"] ?? "59aef810-e681-4b84-bc17-2561fe854c0e";
                var frontendClientId = _configuration["FrontendApp:ClientId"] ?? "f6c2a5e9-3bd5-4223-ad2c-618846a668c5";
                var audience = _configuration["AzureAd:Audience"] ?? $"api://{backendClientId}";

                await _errorLogger.LogAsync(
                    "Configuration loaded",
                    $"TenantId: {tenantId}, BackendClientId: {backendClientId}, FrontendClientId: {frontendClientId}, Audience: {audience}",
                    "NotificationController.SendTestNotification"
                );

                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    await _errorLogger.LogAsync(
                        "Missing or invalid Authorization header",
                        $"Auth header: {authHeader}",
                        "NotificationController.SendTestNotification"
                    );
                    return Unauthorized(new { message = "Missing or invalid Authorization header" });
                }

                var token = authHeader.Substring("Bearer ".Length);
                await _errorLogger.LogAsync(
                    "Processing token validation",
                    $"Token length: {token.Length}",
                    "NotificationController.SendTestNotification"
                );

                // Validate the Azure AD token - accept tokens from frontend for backend audience
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers = new[]
                    {
                        $"https://login.microsoftonline.com/{tenantId}/v2.0",
                        $"https://sts.windows.net/{tenantId}/"
                    },
                    ValidateAudience = true,
                    ValidAudiences = new[]
                    {
                        // Backend API audiences
                        backendClientId,
                        $"api://{backendClientId}",
                        audience,
                        // Frontend client ID (for tokens issued to frontend)
                        frontendClientId,
                        $"api://{frontendClientId}",
                        // Microsoft Graph (for broader compatibility)
                        "00000003-0000-0000-c000-000000000000"
                    },
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                // Load Microsoft signing keys
                using var httpClient = new HttpClient();
                var discoveryUrl = $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration";

                try
                {
                    await _errorLogger.LogAsync(
                        "Loading Microsoft signing keys",
                        $"Discovery URL: {discoveryUrl}",
                        "NotificationController.SendTestNotification"
                    );

                    var discoveryResponse = await httpClient.GetStringAsync(discoveryUrl);
                    var discoveryData = JsonDocument.Parse(discoveryResponse);
                    var jwksUri = discoveryData.RootElement.GetProperty("jwks_uri").GetString();

                    var keysResponse = await httpClient.GetStringAsync(jwksUri!);
                    var signingKeys = new JsonWebKeySet(keysResponse).GetSigningKeys();
                    validationParameters.IssuerSigningKeys = signingKeys;

                    await _errorLogger.LogAsync(
                        "Successfully loaded signing keys",
                        $"Number of keys: {signingKeys.Count()}",
                        "NotificationController.SendTestNotification"
                    );
                }
                catch (Exception keyEx)
                {
                    await _errorLogger.LogAsync(
                        $"Failed to load signing keys: {keyEx.Message}",
                        keyEx.StackTrace ?? "No stack trace",
                        "NotificationController.SendTestNotification"
                    );
                    return StatusCode(500, new { message = "Token validation configuration error", details = keyEx.Message });
                }

                var handler = new JwtSecurityTokenHandler();
                ClaimsPrincipal principal;

                try
                {
                    await _errorLogger.LogAsync(
                        "Starting token validation",
                        $"Valid audiences: {string.Join(", ", validationParameters.ValidAudiences)}",
                        "NotificationController.SendTestNotification"
                    );

                    principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

                    await _errorLogger.LogAsync(
                        "Token validation successful",
                        $"Claims count: {principal.Claims.Count()}",
                        "NotificationController.SendTestNotification"
                    );
                }
                catch (SecurityTokenValidationException validationEx)
                {
                    // Parse the token to see its claims for debugging
                    try
                    {
                        var jwtToken = handler.ReadJwtToken(token);
                        var audClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "aud")?.Value;
                        var issClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "iss")?.Value;
                        
                        await _errorLogger.LogAsync(
                            $"Token validation failed: {validationEx.Message}",
                            $"Token aud: {audClaim}, Token iss: {issClaim}, Expected audiences: {string.Join(", ", validationParameters.ValidAudiences)}",
                            "NotificationController.SendTestNotification"
                        );
                    }
                    catch
                    {
                        await _errorLogger.LogAsync(
                            $"Token validation failed: {validationEx.Message}",
                            validationEx.StackTrace ?? "No stack trace",
                            "NotificationController.SendTestNotification"
                        );
                    }

                    return Unauthorized(new
                    {
                        message = "Invalid or expired token",
                        details = validationEx.Message
                    });
                }
                catch (Exception ex)
                {
                    await _errorLogger.LogAsync(
                        $"Unexpected token validation error: {ex}",
                        ex.StackTrace ?? "No stack trace",
                        "NotificationController.SendTestNotification"
                    );
                    return Unauthorized(new
                    {
                        message = "Token validation failed",
                        details = ex.Message
                    });
                }

                // Extract user ID from token claims BEFORE validation transforms them
                var tokenHandler = new JwtSecurityTokenHandler();
                var rawJwtToken = tokenHandler.ReadJwtToken(token);
                
                // Get the raw Azure AD Object ID from the original JWT token
                var rawAzureAdId = rawJwtToken.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;
                
                await _errorLogger.LogAsync(
                    "Raw JWT token claims inspection",
                    $"Raw oid claim from JWT: {rawAzureAdId}, All claims: {string.Join(", ", rawJwtToken.Claims.Select(c => $"{c.Type}={c.Value}"))}",
                    "NotificationController.SendTestNotification"
                );

                // Extract user ID from validated token claims (for comparison)
                var currentUserId = principal.FindFirst("oid")?.Value ??
                                 principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                                 principal.FindFirst("sub")?.Value;

                var userEmail = principal.FindFirst("upn")?.Value ??
                              principal.FindFirst("preferred_username")?.Value ??
                              principal.FindFirst(ClaimTypes.Email)?.Value;

                var userName = principal.FindFirst("name")?.Value ??
                             principal.FindFirst(ClaimTypes.Name)?.Value ??
                             "Unknown User";

                // Use the raw Azure AD Object ID for Microsoft Graph API calls
                var azureAdObjectId = rawAzureAdId ?? currentUserId;

                await _errorLogger.LogAsync(
                    $"Authenticated user: {userName} ({userEmail})",
                    $"Raw Azure AD Object ID: {azureAdObjectId}, Processed User ID: {currentUserId}",
                    "NotificationController.SendTestNotification"
                );

                if (string.IsNullOrEmpty(azureAdObjectId))
                {
                    await _errorLogger.LogAsync(
                        "User ID not found in token claims",
                        $"Available claims: {string.Join(", ", principal.Claims.Select(c => $"{c.Type}={c.Value}"))}",
                        "NotificationController.SendTestNotification"
                    );
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                // Try to get user from your database (use the raw Azure AD ID for lookup)
                var dbUser = await _userService.GetUserByAzureAdIdAsync(azureAdObjectId);
                var displayName = dbUser?.Name ?? userName;

                try
                {
                    // Send the notification using the correct Azure AD Object ID
                    var message = "Notification invoked! This is a test notification triggered by button click.";

                    await _errorLogger.LogAsync(
                        "Calling notification service - will send from logged-in user to Sandithi",
                        $"Sender (logged-in user): {azureAdObjectId}, Will lookup recipient: sandithin@perituza.com",
                        "NotificationController.SendTestNotification"
                    );
                    try { 
                        await _notificationService.SendNotificationAsync(azureAdObjectId, message);
                    }
                    catch(Exception ex)
                    {
                        await _errorLogger.LogAsync(
                            $"Error in SendNotificationAsync: {ex}",
                            ex.StackTrace ?? "No stack trace",
                            "NotificationController.SendTestNotification"
                        );
                        throw;
                    }

                    await _errorLogger.LogAsync(
                        $"Test notification sent successfully to Azure AD Object ID {azureAdObjectId}",
                        $"User: {displayName}, Message: {message}",
                        "NotificationController.SendTestNotification"
                    );
                }
                catch (Exception notifyEx)
                {
                    await _errorLogger.LogAsync(
                        $"Error sending notification to user {azureAdObjectId}: {notifyEx.Message}",
                        notifyEx.StackTrace ?? "No stack trace",
                        "NotificationController.SendTestNotification"
                    );

                    return StatusCode(500, new
                    {
                        success = false,
                        error = "Failed to send notification",
                        details = notifyEx.Message
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Test notification sent successfully!",
                    userId = currentUserId, // Keep this for compatibility with frontend
                    azureAdObjectId = azureAdObjectId, // Add the correct ID for debugging
                    userName = displayName,
                    userEmail = userEmail,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Unexpected error in SendTestNotification: {ex.Message}",
                    ex.StackTrace ?? "No stack trace",
                    "NotificationController.SendTestNotification"
                );

                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to send notification",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Test endpoint to inspect the JWT token being sent from frontend
        /// </summary>
        [HttpPost("debug-token")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugToken()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No Authorization header found",
                        authHeader = authHeader
                    });
                }

                var token = authHeader.Substring("Bearer ".Length);
                var handler = new JwtSecurityTokenHandler();
                
                try
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    
                    var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
                    
                    await _errorLogger.LogAsync(
                        "Token debug inspection",
                        $"Token claims: {string.Join(", ", claims.Select(kvp => $"{kvp.Key}={kvp.Value}"))}",
                        "NotificationController.DebugToken"
                    );

                    return Ok(new
                    {
                        success = true,
                        message = "Token parsed successfully",
                        tokenInfo = new
                        {
                            header = jwtToken.Header,
                            audience = jwtToken.Claims.FirstOrDefault(c => c.Type == "aud")?.Value,
                            issuer = jwtToken.Claims.FirstOrDefault(c => c.Type == "iss")?.Value,
                            subject = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value,
                            objectId = jwtToken.Claims.FirstOrDefault(c => c.Type == "oid")?.Value,
                            appId = jwtToken.Claims.FirstOrDefault(c => c.Type == "appid")?.Value,
                            allClaims = claims
                        }
                    });
                }
                catch (Exception ex)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Failed to parse token",
                        error = ex.Message,
                        tokenLength = token.Length
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "Token debug failed",
                    details = ex.Message
                });
            }
        }
    }
}