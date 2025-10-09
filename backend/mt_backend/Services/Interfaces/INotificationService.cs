namespace mt_backend.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendTaskCreatedNotificationAsync(string userId, string taskId, string actorName, string taskUrl);
    }

}
