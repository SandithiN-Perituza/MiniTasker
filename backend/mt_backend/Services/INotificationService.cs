using mt_backend.Models;

namespace mt_backend.Services
{
    public interface INotificationService
    {
        Task NotifyUserAsync(User user, string message);
    }
}