using mt_backend.Models;
using mt_backend.DTOs;

public interface IUserService
{
    Task<IEnumerable<User>> GetUsersAsync();
    Task<User> CreateUserAsync(User user);
    Task<User?> LoginAsync(LoginRequest request);
}
