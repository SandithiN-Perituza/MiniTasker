using mt_backend.Models;
using mt_backend.DTOs;

public interface IUserService
{
    Task<IEnumerable<User>> GetUsersAsync();
    Task<User> CreateUserAsync(User user);
    Task<User?> LoginAsync(LoginRequest request);

    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByAzureAdIdAsync(string azureAdId);
    Task<User?> GetUserByIdAsync(int id);

    Task<string> ResolveAzureUserId(int internalUserId);
}
