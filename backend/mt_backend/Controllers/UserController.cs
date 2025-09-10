using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mt_backend.DTOs;
using mt_backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

        [Authorize]
        [HttpPost("msal-login")]
        public async Task<IActionResult> SaveMsalUser()
        {
            Console.WriteLine("=== MSAL Login Attempt ===");

            // Check if the user is authenticated
            Console.WriteLine($"IsAuthenticated: {User.Identity.IsAuthenticated}");
            Console.WriteLine($"Authenticated user name: {User.Identity.Name}");

            // Log all claims from the token
            Console.WriteLine("Token Claims:");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($" - {claim.Type}: {claim.Value}");
            }

            var azureAdId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var displayName = User.FindFirst("name")?.Value;
            var email = User.FindFirst(ClaimTypes.Upn)?.Value ?? "unknown@domain.com";

            if (string.IsNullOrEmpty(azureAdId))
                return BadRequest("Missing Azure AD ID.");

            var existingUser = (await _userService.GetUsersAsync())
                .FirstOrDefault(u => u.AzureAdId == azureAdId);

            if (existingUser == null)
            {
                var newUser = new User
                {
                    AzureAdId = azureAdId,
                    Name = displayName ?? "Unknown",
                    Email = email,
                    Password = "", // Not used for MSAL users
                    CreatedAt = DateTime.UtcNow
                };

                await _userService.CreateUserAsync(newUser);

                Console.WriteLine($"Authenticated user: {User.Identity.Name}");

                return Ok(new
                {
                    message = "Microsoft user saved.",
                    user = newUser
                });
            }

            Console.WriteLine($"Authenticated user: {User.Identity.Name}");

            return Ok(new
            {
                message = "Microsoft user already exists.",
                user = existingUser
            });
        }


        [HttpPost("teams-sso-login")]
        public async Task<IActionResult> TeamsSsoLogin([FromBody] string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var azureAdId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "upn")?.Value;

                if (string.IsNullOrEmpty(azureAdId))
                    return BadRequest("Invalid token.");

                var existingUser = (await _userService.GetUsersAsync())
                    .FirstOrDefault(u => u.AzureAdId == azureAdId);

                if (existingUser == null)
                {
                    var newUser = new User
                    {
                        AzureAdId = azureAdId,
                        Name = name ?? "Unknown",
                        Email = email ?? "unknown@domain.com",
                        Password = "",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _userService.CreateUserAsync(newUser);
                    return Ok(new { message = "User created via SSO", user = newUser });
                }

                return Ok(new { message = "User logged in via SSO", user = existingUser });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = "Token validation failed", details = ex.Message });
            }
        }

    }
}
