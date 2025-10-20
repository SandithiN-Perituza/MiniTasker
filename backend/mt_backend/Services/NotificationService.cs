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
                    $"Error sending notification: {ex}",
                    ex.StackTrace ?? "No stack trace",
                    "NotificationService.SendNotificationAsync"
                );

                Console.WriteLine($"❌ NOTIFICATION ERROR: {ex.Message}");
                
                // Don't re-throw here, as we want the operation to succeed even if notification fails
                Console.WriteLine($"📝 NOTIFICATION (Error Fallback): {message} for user {userId}");
            }
        }

        private async Task SendTeamsNotificationAsync(string senderUserId, string message)
        {
            try
            {
                await _errorLogger.LogAsync(
                    "Starting Teams notification process",
                    $"Sender: {senderUserId}, Message: {message}, UserID Format Check: {(Guid.TryParse(senderUserId, out _) ? "Valid GUID" : "Invalid GUID")}",
                    "NotificationService.SendTeamsNotificationAsync"
                );

                // Get access token for Microsoft Graph
                // NEW CODE:
                var scopes = new[] {
                    "https://graph.microsoft.com/TeamsActivity.Send",
                    "https://graph.microsoft.com/User.Read",
                    "https://graph.microsoft.com/User.Read.All"
                };

                var accessToken = await _graphTokenService!.GetAccessTokenOnBehalfOfAsync(scopes);

                if (string.IsNullOrEmpty(accessToken))
                {
                    await _errorLogger.LogAsync(
                        "No access token available for Graph API",
                        $"Sender: {senderUserId}, Message: {message}",
                        "NotificationService.SendTeamsNotificationAsync"
                    );
                    Console.WriteLine($"📝 NOTIFICATION (No Token): {message} for sender {senderUserId}");
                    return;
                }

                await _errorLogger.LogAsync(
                    "Access token acquired successfully",
                    $"Token length: {accessToken.Length}, Sender: {senderUserId}",
                    "NotificationService.SendTeamsNotificationAsync"
                );

                // Get Sandithi's Azure AD Object ID
                var recipientUserId = await GetSandithiUserId(accessToken);
                
                if (string.IsNullOrEmpty(recipientUserId))
                {
                    await _errorLogger.LogAsync(
                        "Could not find recipient user (Sandithi)",
                        $"Unable to send notification from {senderUserId}",
                        "NotificationService.SendTeamsNotificationAsync"
                    );
                    Console.WriteLine($"❌ RECIPIENT NOT FOUND: Could not find Sandithi's user ID");
                    return;
                }

                // Prevent self-notification
                if (senderUserId.Equals(recipientUserId, StringComparison.OrdinalIgnoreCase))
                {
                    await _errorLogger.LogAsync(
                        "Cannot send notification to self",
                        $"Sender and recipient are the same: {senderUserId}",
                        "NotificationService.SendTeamsNotificationAsync"
                    );
                    Console.WriteLine($"❌ SELF-NOTIFICATION: Cannot send notification to yourself");
                    Console.WriteLine($"📝 NOTIFICATION (Self-notification blocked): {message}");
                    return;
                }

                // Try Teams Activity Feed notification from sender to recipient
                await TryTeamsActivityFeedNotification(senderUserId, recipientUserId, message, accessToken);
            }
            catch (HttpRequestException httpEx)
            {
                await _errorLogger.LogAsync(
                    $"HTTP request error sending Teams notification: {httpEx}",
                    httpEx.StackTrace ?? "No stack trace",
                    "NotificationService.SendTeamsNotificationAsync"
                );
                Console.WriteLine($"🌐 HTTP ERROR: {httpEx.Message}");
                Console.WriteLine($"📝 NOTIFICATION (HTTP Error Fallback): {message} for sender {senderUserId}");
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Error sending Teams notification via Graph API: {ex}",
                    ex.StackTrace ?? "No stack trace",
                    "NotificationService.SendTeamsNotificationAsync"
                );
                Console.WriteLine($"⚠️ GRAPH API ERROR: {ex.Message}");
                Console.WriteLine($"📝 NOTIFICATION (Error Fallback): {message} for sender {senderUserId}");
            }
        }

        private async Task<string?> GetSandithiUserId(string accessToken)
        {
            try
            {
                // Look up Sandithi by email 
                var sandithiEmail = "sandithin@perituza.com";

                await _errorLogger.LogAsync(
                    "Looking up Sandithi's user information",
                    $"Using Email: {sandithiEmail}",
                    "NotificationService.GetSandithiUserId"
                );

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                // Look up by email address
                var emailResponse = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(sandithiEmail)}");
                
                if (emailResponse.IsSuccessStatusCode)
                {
                    var emailContent = await emailResponse.Content.ReadAsStringAsync();
                    var emailUserInfo = JsonConvert.DeserializeObject<dynamic>(emailContent);
                    var actualObjectId = emailUserInfo?.id?.ToString();
                    
                    await _errorLogger.LogAsync(
                        "Found Sandithi by email",
                        $"Object ID: {actualObjectId}, Email: {emailUserInfo?.mail?.ToString() ?? emailUserInfo?.userPrincipalName?.ToString()}",
                        "NotificationService.GetSandithiUserId"
                    );
                    
                    return actualObjectId;
                }

                await _errorLogger.LogAsync(
                    "Could not find Sandithi in Microsoft Graph",
                    $"Email lookup: {emailResponse.StatusCode}, Response: {await emailResponse.Content.ReadAsStringAsync()}",
                    "NotificationService.GetSandithiUserId"
                );

                return null;
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Error looking up Sandithi's user ID: {ex}",
                    ex.StackTrace ?? "No stack trace",
                    "NotificationService.GetSandithiUserId"
                );
                return null;
            }
        }

        private async Task TryTeamsActivityFeedNotification(string senderUserId, string recipientUserId, string message, string accessToken)
        {
            try
            {
                // Build a proper Teams deep link URL
                var appId = "59aef810-e681-4b84-bc17-2561fe854c0e"; // Backend app from webApplicationInfo
                var entityId = "minitasker";
                var actualUrl = "https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net";
                var tabLabel = "MiniTasker";

                var validTeamsUrl = $"https://teams.microsoft.com/l/entity/{appId}/{entityId}?webUrl={Uri.EscapeDataString(actualUrl)}&label={Uri.EscapeDataString(tabLabel)}";

                await _errorLogger.LogAsync(
                    "Sending Teams notification from sender to recipient",
                    $"From: {senderUserId}, To: {recipientUserId}, Teams URL: {validTeamsUrl}",
                    "NotificationService.TryTeamsActivityFeedNotification"
                );

                // Create the notification payload
                var requestBody = new
                {
                    topic = new
                    {
                        source = "text",
                        value = "Task Notification",
                        webUrl = validTeamsUrl
                    },
                    activityType = "systemDefault",
                    previewText = new
                    {
                        content = message
                    },
                    templateParameters = new[]
                    {
                        new { name = "systemDefaultText", value = message }
                    }
                };

                // Serialize the request body
                var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);

                await _errorLogger.LogAsync(
                    "Sending Teams Activity Feed notification",
                    $"URL: https://graph.microsoft.com/v1.0/users/{recipientUserId}/teamwork/sendActivityNotification, Payload: {json}",
                    "NotificationService.TryTeamsActivityFeedNotification"
                );

                // Create HTTP client and send the request to the RECIPIENT
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
                    "NotificationService.TryTeamsActivityFeedNotification"
                );

                if (response.IsSuccessStatusCode)
                {
                    await _errorLogger.LogAsync(
                        $"Teams notification sent successfully from {senderUserId} to {recipientUserId}",
                        $"Message: {message}, Response: {responseContent}",
                        "NotificationService.TryTeamsActivityFeedNotification"
                    );
                    Console.WriteLine($"🚀 TEAMS NOTIFICATION: Successfully sent from {senderUserId} to {recipientUserId}");
                    Console.WriteLine($"   Message: {message}");
                }
                else
                {
                    await _errorLogger.LogAsync(
                        $"Teams Activity Feed failed: {response.StatusCode}",
                        $"Response: {responseContent}, From: {senderUserId}, To: {recipientUserId}",
                        "NotificationService.TryTeamsActivityFeedNotification"
                    );
                    Console.WriteLine($"❌ TEAMS ACTIVITY FEED ERROR: {response.StatusCode}");
                    Console.WriteLine($"   Response: {responseContent}");
                    Console.WriteLine($"   From: {senderUserId}, To: {recipientUserId}");
                    
                    // Verify both users exist
                    await TrySimpleUserNotification(senderUserId, recipientUserId, message, accessToken);
                }
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Exception in Teams Activity Feed notification: {ex.Message}",
                    ex.StackTrace ?? "No stack trace",
                    "NotificationService.TryTeamsActivityFeedNotification"
                );
                
                // Try user verification as fallback
                await TrySimpleUserNotification(senderUserId, recipientUserId, message, accessToken);
            }
        }

        private async Task TrySimpleUserNotification(string senderUserId, string recipientUserId, string message, string accessToken)
        {
            try
            {
                await _errorLogger.LogAsync(
                    "Attempting simple user information retrieval as fallback",
                    $"Sender: {senderUserId}, Recipient: {recipientUserId}, Message: {message}",
                    "NotificationService.TrySimpleUserNotification"
                );

                // As a fallback, try to get user information to verify the user exists
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var userResponse = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{recipientUserId}");
                var userContent = await userResponse.Content.ReadAsStringAsync();

                await _errorLogger.LogAsync(
                    $"User lookup response: {userResponse.StatusCode}",
                    $"User exists check: {userContent}",
                    "NotificationService.TrySimpleUserNotification"
                );

                if (userResponse.IsSuccessStatusCode)
                {
                    await _errorLogger.LogAsync(
                        $"User {recipientUserId} exists in Microsoft Graph - notification would be deliverable",
                        $"Message: {message}, User info retrieved successfully",
                        "NotificationService.TrySimpleUserNotification"
                    );
                    Console.WriteLine($"✅ USER VERIFIED: User {recipientUserId} exists in Microsoft Graph");
                    Console.WriteLine($"📝 NOTIFICATION (Verified User): {message} for user {recipientUserId}");
                    Console.WriteLine($"   Note: Teams notifications require proper Teams app registration or alternative delivery method");
                }
                else
                {
                    await _errorLogger.LogAsync(
                        $"User {recipientUserId} not found or accessible: {userResponse.StatusCode}",
                        $"Response: {userContent}",
                        "NotificationService.TrySimpleUserNotification"
                    );
                    Console.WriteLine($"❌ USER NOT FOUND: {userResponse.StatusCode}");
                    Console.WriteLine($"📝 NOTIFICATION (User Not Found): {message} for user {recipientUserId}");
                }
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Exception in simple user notification: {ex.Message}",
                    ex.StackTrace ?? "No stack trace",
                    "NotificationService.TrySimpleUserNotification"
                );
                Console.WriteLine($"📝 NOTIFICATION (Final Fallback): {message} for user {recipientUserId}");
            }
        }
    }
}