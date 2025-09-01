using mt_backend.Services.Interfaces;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace mt_backend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;

        public NotificationService(string webhookUrl)
        {
            _httpClient = new HttpClient();
            _webhookUrl = webhookUrl;
        }

        public async Task SendMessageAsync(string message)
        {
            var payload = $"{{ \"text\": \"{message}\" }}";
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_webhookUrl, content);
            response.EnsureSuccessStatusCode();
        }
    }
}

