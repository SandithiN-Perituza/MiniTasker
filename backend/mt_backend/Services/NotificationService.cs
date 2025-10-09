using mt_backend.Services.Interfaces;
using System.Threading.Tasks;

namespace mt_backend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IErrorLogger _errorLogger;

        public NotificationService(IErrorLogger errorLogger)
        {
            _errorLogger = errorLogger;
        }

        public async Task SendTaskCreatedNotificationAsync(string userId, string taskId, string actorName, string taskUrl)
        {
            // TODO: Implement Microsoft Graph notification when proper dependencies are configured
            // For now, just log that a notification would be sent
            await _errorLogger.LogAsync(
                $"Notification would be sent to user {userId} for task {taskId}",
                $"Actor: {actorName}, URL: {taskUrl}",
                "NotificationService.SendTaskCreatedNotificationAsync"
            );
            
            // This prevents the 500 error and allows task fetching to work
            // You can implement the actual Graph API call when dependencies are properly set up
        }
    }
}