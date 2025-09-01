namespace mt_backend.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendMessageAsync(string message);
    }

}
