namespace mt_backend.Services.Interfaces
{
    public interface IGraphTokenService
    {
        Task<string> GetAccessTokenOnBehalfOfAsync(string[] scopes);
    }
}
