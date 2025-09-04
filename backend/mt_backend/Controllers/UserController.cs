using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mt_backend.DTOs;
using mt_backend.Models;
using mt_backend.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace mt_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //This protects all actions in this controller
    //[Authorize] 
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _userService.GetUsersAsync();

            var userResponses = users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            });

            return Ok(userResponses);
        }

        // POST: api/users
        [HttpPost]
        // This allows login without authentication
        [AllowAnonymous] 
        public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] CreateUserRequestDto request)
        {
            var newUser = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = request.Password
            };

            var createdUser = await _userService.CreateUserAsync(newUser);

            var response = new UserResponseDto
            {
                Id = createdUser.Id,
                Name = createdUser.Name,
                Email = createdUser.Email
            };

            return CreatedAtAction(nameof(GetUsers), new { id = response.Id }, response);
        }

        // POST: api/users/login
        [HttpPost("login")]
        // This allows login without authentication
        [AllowAnonymous] 
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.LoginAsync(request);

            if (user == null)
                return Unauthorized("Invalid email or password.");

            var response = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };

            return Ok(response);
        }

        // /api/users/msal-login
        [Authorize]
        [HttpPost("msal-login")]
        public async Task<IActionResult> SaveMsalUser()
        {
            var azureAdId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var name = User.FindFirstValue(ClaimTypes.Name);
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(azureAdId))
                return BadRequest("Missing Azure AD ID.");

            var existingUser = (await _userService.GetUsersAsync())
                .FirstOrDefault(u => u.AzureAdId == azureAdId);

            if (existingUser == null)
            {
                var newUser = new User
                {
                    AzureAdId = azureAdId,
                    Name = name ?? "Unknown",
                    Email = email ?? "unknown@domain.com",
                    Password = "", // Not used for MSAL users
                    CreatedAt = DateTime.UtcNow
                };

                await _userService.CreateUserAsync(newUser);
            }
            Console.WriteLine($"Authenticated user: {User.Identity.Name}");
            return Ok("Microsoft user saved.");

        }


    }
}
