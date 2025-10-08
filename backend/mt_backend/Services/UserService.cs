using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using mt_backend.Data;
using mt_backend.DTOs;
using mt_backend.Models;

namespace mt_backend.Services
{
    public class UserService : IUserService
    {
        private readonly MiniTaskerDbContext _context;

        //PasswordHasher - provided by Microsoft.AspNetCore.Identity
        private readonly PasswordHasher<User> _hasher = new();

        public UserService(MiniTaskerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            if (!string.IsNullOrEmpty(user.Password))
            {
                user.Password = _hasher.HashPassword(user, user.Password);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return null;

            var result = _hasher.VerifyHashedPassword(user, user.Password, request.Password);
            return result == PasswordVerificationResult.Failed ? null : user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<User?> GetUserByAzureAdIdAsync(string azureAdId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.AzureAdId == azureAdId);
        }
    }

}
