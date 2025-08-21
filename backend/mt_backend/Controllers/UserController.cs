using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mt_backend.Data;
using mt_backend.DTOs;
using mt_backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mt_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly MiniTaskerDbContext _context;

        public UsersController(MiniTaskerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, user.Password);
            Console.WriteLine($"signin user: {user}, password: {user.Password}");

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Console.WriteLine($"Login attempt for request: {request}");
            Console.WriteLine($"Login attempt for email: {request.Email}");
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            Console.WriteLine($"Login attempt: {request.Email} / {request.Password}");

            if (user == null)
                return Unauthorized("User not found.Invalid email or password.");

            Console.WriteLine($"Stored hash: {user.Password}");

            try
            {
                var hasher = new PasswordHasher<User>();
                var result = hasher.VerifyHashedPassword(user, user.Password, request.Password);
                Console.WriteLine($"Password verification result: {result}");

                if (result == PasswordVerificationResult.Failed)
                    return Unauthorized("Password verification failed. Invalid email or password.");

                return Ok(new
                {
                    user.Id,
                    user.Name,
                    user.Email
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during password verification: {ex.Message}");
                return StatusCode(500, "Internal server error during password verification.");
            }

        }

    }
}
