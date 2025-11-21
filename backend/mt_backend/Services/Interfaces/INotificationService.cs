using mt_backend.DTOs;

namespace mt_backend.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message);

        // Updated to return NotificationResultDto
        Task<NotificationResultDto> SendNotificationWithTokenAsync(string recipientAzureAdId, string message, string accessToken, string? senderName = null);

        // Updated to return NotificationResultDto
        Task<NotificationResultDto> SendNotificationWithOBOTokenAsync(string recipientAzureAdId, string message, string userAccessToken, string? senderName = null);
    }
}