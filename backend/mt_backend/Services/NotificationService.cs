using mt_backend.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class NotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly IErrorLogger _errorLogger;

    public NotificationService(IHttpClientFactory httpClientFactory, IErrorLogger errorLogger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _errorLogger = errorLogger;
    }

    public async Task SendTaskCreatedNotificationAsync(string userId, string taskId, string actorName, string taskUrl)
    {
        var accessToken = await GraphTokenProvider.GetAccessTokenAsync();

        taskUrl = $"https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net/?highlight={taskId}";

        var requestUrl = $"https://graph.microsoft.com/beta/users/{userId}/teamwork/sendActivityNotification";

        var payload = new
        {
            topic = new
            {
                source = "entityUrl",
                value = taskUrl,
                webUrl = taskUrl
            },
            activityType = "taskCreated",
            previewText = new { content = "You have a new task assigned." },
            templateParameters = new[]
            {
                new { name = "taskId", value = taskId },
                new { name = "actor", value = actorName }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            await _errorLogger.LogAsync($"Graph API Error: {response.StatusCode}", errorContent, "NotificationService.SendTaskCreatedNotificationAsync");
        }

        response.EnsureSuccessStatusCode();
    }
}
