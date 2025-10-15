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

                // Get tenant ID and client ID from configuration
                var tenantId = _configuration["AzureAd:TenantId"] ?? "7b967b11-c0b9-402b-b483-d694f50dfb82";
                var clientId = _configuration["AzureAd:ClientId"] ?? "59aef810-e681-4b84-bc17-2561fe854c0e";

                await _errorLogger.LogAsync(
                    "Configuration loaded",
                    $"TenantId: {tenantId}, ClientId: {clientId}",
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

                // Validate the Azure AD token
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
                        clientId,
                        "00000003-0000-0000-c000-000000000000", // Microsoft Graph
                        $"api://{clientId}"
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
                        "Validating JWT token",
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
                    await _errorLogger.LogAsync(
                        $"Token validation failed: {validationEx.Message}",
                        validationEx.StackTrace ?? "No stack trace",
                        "NotificationController.SendTestNotification"
                    );
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

                // Extract user ID from token claims
                var currentUserId = principal.FindFirst("oid")?.Value ??
                                 principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                                 principal.FindFirst("sub")?.Value;

                var userEmail = principal.FindFirst("upn")?.Value ??
                              principal.FindFirst("preferred_username")?.Value ??
                              principal.FindFirst(ClaimTypes.Email)?.Value;

                var userName = principal.FindFirst("name")?.Value ??
                             principal.FindFirst(ClaimTypes.Name)?.Value ??
                             "Unknown User";

                if (string.IsNullOrEmpty(currentUserId))
                {
                    await _errorLogger.LogAsync(
                        "User ID not found in token claims",
                        $"Available claims: {string.Join(", ", principal.Claims.Select(c => c.Type))}",
                        "NotificationController.SendTestNotification"
                    );
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                await _errorLogger.LogAsync(
                    $"Authenticated user: {userName} ({userEmail})",
                    $"User ID: {currentUserId}",
                    "NotificationController.SendTestNotification"
                );

                // Try to get user from your database (optional)
                var dbUser = await _userService.GetUserByAzureAdIdAsync(currentUserId);
                var displayName = dbUser?.Name ?? userName;

                try
                {
                    // Send the notification
                    var message = "Notification invoked! This is a test notification triggered by button click.";

                    await _errorLogger.LogAsync(
                        "Calling notification service",
                        $"About to send notification to user {currentUserId}",
                        "NotificationController.SendTestNotification"
                    );
                    try { 
                        await _notificationService.SendNotificationAsync(currentUserId, message);
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
                        $"Test notification sent successfully to user {currentUserId}",
                        $"User: {displayName}, Message: {message}",
                        "NotificationController.SendTestNotification"
                    );
                }
                catch (Exception notifyEx)
                {
                    await _errorLogger.LogAsync(
                        $"Error sending notification to user {currentUserId}: {notifyEx.Message}",
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
                    userId = currentUserId,
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
    }
}