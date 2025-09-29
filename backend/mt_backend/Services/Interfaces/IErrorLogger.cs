namespace mt_backend.Services.Interfaces
{
    public interface IErrorLogger
    {
        Task LogAsync(string message, string stackTrace, string source);
    }
}
