namespace mt_backend.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message);

        // New method for sending notifications with delegated permissions
        Task SendNotificationWithTokenAsync(string recipientAzureAdId, string message, string accessToken, string? senderName = null);
    }
}
