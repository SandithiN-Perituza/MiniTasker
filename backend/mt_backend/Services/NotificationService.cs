using mt_backend.Services.Interfaces;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace mt_backend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IErrorLogger _errorLogger;
        private readonly IGraphTokenService? _graphTokenService;

        public NotificationService(IErrorLogger errorLogger, IGraphTokenService? graphTokenService = null)
        {
            _errorLogger = errorLogger;
            _graphTokenService = graphTokenService;
        }

        public async Task SendNotificationAsync(string userId, string message)
        {
            try
            {
                await _errorLogger.LogAsync(
                    $"Attempting to send notification to user {userId}",
                    $"Message: {message}, GraphTokenService available: {_graphTokenService != null}",
                    "NotificationService.SendNotificationAsync"
                );

                // Check if Microsoft Graph API is available
                if (_graphTokenService != null)
                {
                    await _errorLogger.LogAsync(
                        "Graph API available, attempting to send Teams notification",
                        $"User: {userId}, Message: {message}",
                        "NotificationService.SendNotificationAsync"
                    );

                    // Use Microsoft Graph API to send Teams notification
                    await SendTeamsNotificationAsync(userId, message);
                }
                else
                {
                    // Fallback to logging when Graph API is not available
                    await _errorLogger.LogAsync(
                        $"Graph API not available - notification logged instead",
                        $"User: {userId}, Message: {message}",
                        "NotificationService.SendNotificationAsync"
                    );
                    Console.WriteLine($"📝 NOTIFICATION (Logged): {message} for user {userId}");
                }
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Error sending notification: {ex.Message}",
                    ex.StackTrace ?? "No stack trace",
                    "NotificationService.SendNotificationAsync"
                );

                Console.WriteLine($"❌ NOTIFICATION ERROR: {ex.Message}");
                
                // Don't re-throw here, as we want the operation to succeed even if notification fails
                Console.WriteLine($"📝 NOTIFICATION (Error Fallback): {message} for user {userId}");
            }
        }

        private async Task SendTeamsNotificationAsync(string userId, string message)
        {
            try
            {
                await _errorLogger.LogAsync(
                    "Starting Teams notification process",
                    $"User: {userId}, Message: {message}",
                    "NotificationService.SendTeamsNotificationAsync"
                );

                // Get access token for Microsoft Graph
                var scopes = new[] {
                    "https://graph.microsoft.com/TeamsActivity.Send",
                    "https://graph.microsoft.com/User.Read"
                };

                var accessToken = await _graphTokenService!.GetAccessTokenOnBehalfOfAsync(scopes);

                if (string.IsNullOrEmpty(accessToken))
                {
                    await _errorLogger.LogAsync(
                        "No access token available for Graph API",
                        $"User: {userId}, Message: {message}",
                        "NotificationService.SendTeamsNotificationAsync"
                    );
                    Console.WriteLine($"📝 NOTIFICATION (No Token): {message} for user {userId}");
                    return;
                }

                await _errorLogger.LogAsync(
                    "Access token acquired successfully",
                    $"Token length: {accessToken.Length}, User: {userId}",
                    "NotificationService.SendTeamsNotificationAsync"
                );

                // Build the notification URL (you can customize this)
                var notificationUrl = "https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net/";

                // Prepare the Microsoft Graph API request payload
                var requestBody = new
                {
                    topic = new
                    {
                        source = "entityUrl",
                        value = notificationUrl,
                        webUrl = notificationUrl
                    },
                    activityType = "userNotification", // Custom activity type
                    previewText = new
                    {
                        content = message
                    },
                    templateParameters = new[]
                    {
                        new { name = "notificationMessage", value = message },
                        new { name = "timestamp", value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
                        new { name = "userId", value = userId }
                    }
                };

                // Serialize the request body
                var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);

                await _errorLogger.LogAsync(
                    "Sending request to Microsoft Graph API",
                    $"URL: https://graph.microsoft.com/v1.0/users/{userId}/teamwork/sendActivityNotification, Payload length: {json.Length}",
                    "NotificationService.SendTeamsNotificationAsync"
                );

                // Create HTTP client and send the request
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(
                    $"https://graph.microsoft.com/v1.0/users/{userId}/teamwork/sendActivityNotification",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    await _errorLogger.LogAsync(
                        $"Teams notification sent successfully via Graph API to user {userId}",
                        $"Message: {message}",
                        "NotificationService.SendTeamsNotificationAsync"
                    );
                    Console.WriteLine($"🚀 TEAMS NOTIFICATION: Successfully sent to user {userId}");
                    Console.WriteLine($"   Message: {message}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await _errorLogger.LogAsync(
                        $"Microsoft Graph API Error: {response.StatusCode}",
                        $"Response: {errorContent}, User: {userId}, Message: {message}",
                        "NotificationService.SendTeamsNotificationAsync"
                    );
                    Console.WriteLine($"❌ GRAPH API ERROR: {response.StatusCode}");
                    Console.WriteLine($"   Response: {errorContent}");

                    // Fallback to logging the notification
                    Console.WriteLine($"📝 NOTIFICATION (Fallback): {message} for user {userId}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                await _errorLogger.LogAsync(
                    $"HTTP request error sending Teams notification: {httpEx}",
                    httpEx.StackTrace ?? "No stack trace",
                    "NotificationService.SendTeamsNotificationAsync"
                );
                Console.WriteLine($"🌐 HTTP ERROR: {httpEx.Message}");
                Console.WriteLine($"📝 NOTIFICATION (HTTP Error Fallback): {message} for user {userId}");
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Error sending Teams notification via Graph API: {ex}",
                    ex.StackTrace ?? "No stack trace",
                    "NotificationService.SendTeamsNotificationAsync"
                );
                Console.WriteLine($"⚠️ GRAPH API ERROR: {ex.Message}");
                Console.WriteLine($"📝 NOTIFICATION (Error Fallback): {message} for user {userId}");
            }
        }
    }
}