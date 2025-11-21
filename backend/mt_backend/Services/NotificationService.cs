using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using mt_backend.DTOs;
using mt_backend.Services.Interfaces;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;

namespace mt_backend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IErrorLogger _errorLogger;
        private readonly IGraphTokenService? _graphTokenService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IErrorLogger errorLogger, IGraphTokenService? graphTokenService = null, ILogger<NotificationService>? logger = null)
        {
            _errorLogger = errorLogger;
            _graphTokenService = graphTokenService;
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<NotificationService>.Instance;
        }

        // High-level entry: keep this to remain compatible with callers
        public async Task SendNotificationAsync(string userId, string message)
        {
            try
            {
                await _errorLogger.LogAsync(
                    $"Attempting to send notification to user {userId}",
                    $"Message: {message}, GraphTokenService available: {_graphTokenService != null}",
                    "NotificationService.SendNotificationAsync"
                );

                if (_graphTokenService != null)
                {
                    // This method will internally choose OBO or app-only or fail gracefully
                    await SendTeamsNotificationAsync(userId, message);
                    return;
                }

                // Fallback logging-only behaviour
                await _errorLogger.LogAsync(
                    $"Graph API not available - notification logged instead",
                    $"User: {userId}, Message: {message}",
                    "NotificationService.SendNotificationAsync"
                );
                Console.WriteLine($"📝 NOTIFICATION (Logged): {message} for user {userId}");
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Error sending notification: {ex.Message}",
                    ex.StackTrace ?? "No stack trace",
                    "NotificationService.SendNotificationAsync"
                );
                Console.WriteLine($"❌ NOTIFICATION ERROR: {ex.Message}");
            }
        }

        // Internal: prefer server-side OBO using current context if available (this method is used by controller flows)
        private async Task SendTeamsNotificationAsync(string senderUserId, string message)
        {
            try
            {
                await _errorLogger.LogAsync(
                    "Starting Teams notification process",
                    $"Sender: {senderUserId}, Message: {message}",
                    "NotificationService.SendTeamsNotificationAsync"
                );

                // Scopes for activity feed
                var scopes = new[] { "https://graph.microsoft.com/TeamsActivity.Send" };

                // Try to obtain a Graph token using the IGraphTokenService (ITokenAcquisition or manual OBO)
                string? accessToken = null;

                try
                {
                    // This variant expects the GraphTokenService's GetAccessTokenOnBehalfOfAsync(scopes)
                    // to use the ambient HttpContext principal when available.
                    accessToken = await _graphTokenService!.GetAccessTokenOnBehalfOfAsync(scopes);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ambient OBO attempt failed - will expect controllers to call SendNotificationWithOBOTokenAsync when they have a user token.");
                }

                if (string.IsNullOrEmpty(accessToken))
                {
                    await _errorLogger.LogAsync(
                        "Ambient OBO returned no token - skipping direct server-side send here",
                        $"Sender: {senderUserId}",
                        "NotificationService.SendTeamsNotificationAsync"
                    );
                    return;
                }

                // Find recipient (example uses Sandithi lookup; you probably want recipient by assigned user)
                var recipientUserId = await GetSandithiUserId(accessToken);
                if (string.IsNullOrEmpty(recipientUserId))
                {
                    _logger.LogWarning("Recipient not found; aborting send");
                    return;
                }

                // Prevent self notify
                if (senderUserId.Equals(recipientUserId, StringComparison.OrdinalIgnoreCase))
                {
                    await _errorLogger.LogAsync("Cannot send notification to self", $"Sender: {senderUserId}", "NotificationService.SendTeamsNotificationAsync");
                    return;
                }

                // Use the internal method to send the notification
                await SendTeamsNotificationWithTokenInternalAsync(recipientUserId, message, accessToken, senderUserId);
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Error sending Teams notification: {ex.Message}",
                    ex.StackTrace ?? "No stack trace",
                    "NotificationService.SendTeamsNotificationAsync"
                );
                _logger.LogError(ex, "SendTeamsNotificationAsync failed");
            }
        }

        // Called by controller when it has a user token (explicit OBO)
        public async Task<NotificationResultDto> SendNotificationWithOBOTokenAsync(string recipientAzureAdId, string message, string userAccessToken, string? senderName = null)
        {
            var result = new NotificationResultDto
            {
                Method = "SendNotificationWithOBOTokenAsync (OBO Token Exchange)",
                TokenType = "OBO Exchange"
            };

            try
            {
                await _errorLogger.LogAsync(
                    "Starting OBO flow for notification",
                    $"Recipient: {recipientAzureAdId}, Sender: {senderName ?? "Unknown"}, UserTokenPresent: {!string.IsNullOrEmpty(userAccessToken)}",
                    "NotificationService.SendNotificationWithOBOTokenAsync"
                );

                result.RecipientId = recipientAzureAdId;
                result.SenderName = senderName;
                result.Message = message;

                if (string.IsNullOrEmpty(userAccessToken))
                {
                    _logger.LogWarning("No user access token supplied for OBO");
                    result.Success = false;
                    result.ErrorDetails = "No user access token supplied for OBO";
                    return result;
                }

                if (_graphTokenService == null)
                {
                    _logger.LogWarning("GraphTokenService not configured; cannot perform OBO");
                    result.Success = false;
                    result.ErrorDetails = "GraphTokenService not configured; cannot perform OBO";
                    return result;
                }

                var scopes = new[] { "https://graph.microsoft.com/TeamsActivity.Send" };
                string? graphToken = null;

                try
                {
                    graphToken = await _graphTokenService.GetAccessTokenOnBehalfOfAsync(scopes, userAccessToken);
                }
                catch (Exception ex)
                {
                    await _errorLogger.LogAsync("OBO exchange failed", ex.Message, "NotificationService.SendNotificationWithOBOTokenAsync");
                    _logger.LogError(ex, "OBO exchange failed");
                    result.Success = false;
                    result.ErrorDetails = $"OBO exchange failed: {ex.Message}";
                    return result;
                }

                if (string.IsNullOrEmpty(graphToken))
                {
                    _logger.LogWarning("OBO produced no Graph token; skipping notification");
                    result.Success = false;
                    result.ErrorDetails = "OBO produced no Graph token";
                    return result;
                }

                // Call the actual notification sending
                var sendResult = await SendTeamsNotificationWithTokenInternalAsync(recipientAzureAdId, message, graphToken, senderName);

                result.Success = sendResult.Success;
                result.ErrorDetails = sendResult.ErrorDetails;
                if (sendResult.Success)
                {
                    result.Message = $"Notification sent successfully via OBO to {recipientAzureAdId}";
                }

                return result;
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Error in OBO notification flow: {ex.Message}",
                    ex.StackTrace ?? "No stack trace",
                    "NotificationService.SendNotificationWithOBOTokenAsync"
                );
                _logger.LogError(ex, "SendNotificationWithOBOTokenAsync failed");

                result.Success = false;
                result.ErrorDetails = ex.Message;
                return result;
            }
        }

        // Called when the caller provides a Graph token (client-acquired or returned by token-exchange)
        public async Task<NotificationResultDto> SendNotificationWithTokenAsync(string recipientAzureAdId, string message, string accessToken, string? senderName = null)
        {
            var result = new NotificationResultDto
            {
                Method = "SendNotificationWithTokenAsync (Direct Graph Token)",
                TokenType = "Direct Graph Token"
            };

            try
            {
                await _errorLogger.LogAsync(
                    "Sending notification with provided Graph token",
                    $"Recipient: {recipientAzureAdId}, Sender: {senderName ?? "Unknown"}",
                    "NotificationService.SendNotificationWithTokenAsync"
                );

                result.RecipientId = recipientAzureAdId;
                result.SenderName = senderName;
                result.Message = message;

                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogWarning("No access token supplied to SendNotificationWithTokenAsync");
                    result.Success = false;
                    result.ErrorDetails = "No access token supplied";
                    return result;
                }

                // Basic validation of token claims (avoid using invalid token)
                if (!IsLikelyGraphToken(accessToken))
                {
                    await _errorLogger.LogAsync("Provided token failed basic Graph validation", $"Token length: {accessToken.Length}", "NotificationService.SendNotificationWithTokenAsync");
                    _logger.LogWarning("Provided token appears not to be a Graph token");
                    result.Success = false;
                    result.ErrorDetails = "Provided token appears not to be a Graph token";
                    return result;
                }

                // Prevent self notification if token contains sender oid
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(accessToken);
                    var senderOid = jwt.Claims.FirstOrDefault(c => c.Type == "oid")?.Value ?? jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                    if (!string.IsNullOrEmpty(senderOid) && senderOid.Equals(recipientAzureAdId, StringComparison.OrdinalIgnoreCase))
                    {
                        await _errorLogger.LogAsync("Self-notification detected - skipping", $"SenderOid: {senderOid}", "NotificationService.SendNotificationWithTokenAsync");
                        result.Success = false;
                        result.ErrorDetails = "Self-notification detected - skipping";
                        return result;
                    }
                }
                catch
                {
                    // ignore parse errors; still attempt send
                }

                // Call the actual notification sending
                var sendResult = await SendTeamsNotificationWithTokenInternalAsync(recipientAzureAdId, message, accessToken, senderName);

                result.Success = sendResult.Success;
                result.ErrorDetails = sendResult.ErrorDetails;
                if (sendResult.Success)
                {
                    result.Message = $"Notification sent successfully to {recipientAzureAdId}";
                }

                return result;
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Error sending notification with token: {ex.Message}",
                    ex.StackTrace ?? "No stack trace",
                    "NotificationService.SendNotificationWithTokenAsync"
                );
                _logger.LogError(ex, "SendNotificationWithTokenAsync failed");

                result.Success = false;
                result.ErrorDetails = ex.Message;
                return result;
            }
        }

        // Internal method that actually sends the notification and returns success/failure
        private async Task<(bool Success, string? ErrorDetails)> SendTeamsNotificationWithTokenInternalAsync(string recipientUserId, string message, string accessToken, string? senderName = null)
        {
            try
            {
                var appId = "f6c2a5e9-3bd5-4223-ad2c-618846a668c5";
                var entityId = "minitasker";
                var actualUrl = "https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net";
                var tabLabel = "MiniTasker";

                var validTeamsUrl = $"https://teams.microsoft.com/l/entity/{appId}/{entityId}?webUrl={Uri.EscapeDataString(actualUrl)}&label={Uri.EscapeDataString(tabLabel)}";

                var notificationMessage = !string.IsNullOrEmpty(senderName) ? $"{senderName}: {message}" : message;

                await _errorLogger.LogAsync(
                    "Sending Teams Activity Feed notification",
                    $"To: {recipientUserId}, Message: {notificationMessage}",
                    "NotificationService.SendTeamsNotificationWithTokenInternalAsync"
                );

                var requestBody = new
                {
                    topic = new
                    {
                        source = "text",
                        value = "Task Assignment Notification",
                        webUrl = validTeamsUrl
                    },
                    activityType = "systemDefault",
                    previewText = new { content = notificationMessage },
                    templateParameters = new[] { new { name = "systemDefaultText", value = notificationMessage } }
                };

                var json = JsonConvert.SerializeObject(requestBody);

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(
                    $"https://graph.microsoft.com/v1.0/users/{recipientUserId}/teamwork/sendActivityNotification",
                    content);

                var responseContent = await response.Content.ReadAsStringAsync();

                await _errorLogger.LogAsync(
                    $"Teams Activity Feed Response: {response.StatusCode}",
                    $"Response Content: {responseContent}",
                    "NotificationService.SendTeamsNotificationWithTokenInternalAsync"
                );

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Teams Activity Feed failed: {Status} {Content}", response.StatusCode, responseContent);
                    return (false, $"Teams Activity Feed failed: {response.StatusCode} - {responseContent}");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Exception sending Teams notification with token: {ex.Message}",
                    ex.StackTrace ?? "No stack trace",
                    "NotificationService.SendTeamsNotificationWithTokenInternalAsync"
                );
                _logger.LogError(ex, "SendTeamsNotificationWithTokenInternalAsync exception");
                return (false, ex.Message);
            }
        }

        // Helper: look up a specific user by email (kept for your Sandithi flow)
        private async Task<string?> GetSandithiUserId(string accessToken)
        {
            try
            {
                var sandithiEmail = "sandithin@perituza.com";
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var emailResponse = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(sandithiEmail)}");
                if (emailResponse.IsSuccessStatusCode)
                {
                    var emailContent = await emailResponse.Content.ReadAsStringAsync();
                    dynamic emailUserInfo = JsonConvert.DeserializeObject<dynamic>(emailContent);
                    return (string?)emailUserInfo?.id;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSandithiUserId failed");
                return null;
            }
        }

        private async Task TrySimpleUserNotification(string senderUserId, string recipientUserId, string message, string accessToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var userResponse = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{recipientUserId}");
                var userContent = await userResponse.Content.ReadAsStringAsync();

                if (userResponse.IsSuccessStatusCode)
                {
                    await _errorLogger.LogAsync($"User {recipientUserId} exists in Graph", userContent, "NotificationService.TrySimpleUserNotification");
                    Console.WriteLine($"✅ USER VERIFIED: {recipientUserId}");
                }
                else
                {
                    await _errorLogger.LogAsync($"User lookup failed: {userResponse.StatusCode}", userContent, "NotificationService.TrySimpleUserNotification");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TrySimpleUserNotification failed");
            }
        }

        // Basic heuristic: check token has aud claim for Graph or known Graph app id
        private bool IsLikelyGraphToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var aud = jwt.Claims.FirstOrDefault(c => c.Type == "aud")?.Value ?? string.Empty;
                return aud.Contains("graph.microsoft.com") || aud == "00000003-0000-0000-c000-000000000000";
            }
            catch
            {
                return false;
            }
        }
    }
}