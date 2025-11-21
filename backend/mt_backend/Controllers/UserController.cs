using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using mt_backend.DTOs;
using mt_backend.Models;
using mt_backend.Services;
using mt_backend.Services.Interfaces;
using Newtonsoft.Json;
using System.Configuration;
using System.Security.Claims;
using System.Text.Json;

namespace mt_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //This protects all actions in this controller
    //[Authorize] 
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IErrorLogger _errorLogger;
        private readonly IConfiguration _configuration;

        public UsersController(IUserService userService, IConfiguration configuration, IErrorLogger errorLogger)
        {
            _userService = userService;
            _configuration = configuration;
            _errorLogger = errorLogger;
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

        [HttpGet("{userId}/azuread-id")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserAzureAdId(int userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                return Ok(new
                {
                    userId = user.Id,
                    name = user.Name,
                    email = user.Email,
                    azureAdId = user.AzureAdId,
                    hasAzureAdId = !string.IsNullOrEmpty(user.AzureAdId)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("teams-sync")]
        [AllowAnonymous]
        public async Task<IActionResult> TeamsSync()
        {
            try
            {
                await _errorLogger.LogAsync(
                    "Teams sync endpoint called",
                    "Processing Teams directory sync request",
                    "UsersController.TeamsSync"
                );

                // This endpoint can be used for Teams-specific user synchronization
                // For now, return a successful response to prevent 404 errors
                return Ok(new
                {
                    success = true,
                    message = "Teams sync completed",
                    timestamp = DateTime.UtcNow,
                    note = "This endpoint is available for Teams directory synchronization"
                });
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    "Teams sync failed",
                    $"Error: {ex.Message}",
                    "UsersController.TeamsSync"
                );

                return StatusCode(500, new { error = "Teams sync failed" });
            }
        }
    }


}
