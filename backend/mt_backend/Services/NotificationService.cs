using System.Threading.Tasks;
using mt_backend.Models;
using Microsoft.Extensions.Logging;

namespace mt_backend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public Task NotifyUserAsync(User user, string message)
        {
            // Replace with actual notification logic (email, SMS, etc.)
            _logger.LogInformation($"Notification to {user.Email}: {message}");
            return Task.CompletedTask;
        }
    }
}