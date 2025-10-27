//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
//using mt_backend.Models;
//using mt_backend.Services.Interfaces;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Threading.Tasks;

//namespace mt_backend.Controllers
//{
//    [ApiController]
//    [Route("api/auth")]
//    public class MicrosoftAuthController : ControllerBase
//    {
//        private readonly IUserService _userService;

//        public MicrosoftAuthController(IUserService userService)
//        {
//            _userService = userService;
//        }

//        [HttpPost("microsoft")]
//        public async Task<IActionResult> MicrosoftLogin()
//        {
//            var authHeader = Request.Headers["Authorization"].ToString();
//            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
//                return Unauthorized("Missing or invalid token");

//            var token = authHeader.Substring("Bearer ".Length);

//            var handler = new JwtSecurityTokenHandler();
//            JwtSecurityToken jwtToken;

//            try
//            {
//                jwtToken = handler.ReadJwtToken(token);
//            }
//            catch
//            {
//                return Unauthorized("Invalid token format");
//            }

//            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
//            var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

//            var azureAdId = jwtToken.Claims.FirstOrDefault(c => c.Type == "oid")?.Value ??
//                jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

//            if (string.IsNullOrEmpty(email))
//                return Unauthorized("Email not found in token");

//            if (string.IsNullOrEmpty(azureAdId))
//                return Unauthorized("Azure AD ID not found in token");

//            //var user = await _userService.GetUserByEmailAsync(email);
//            var user = await _userService.GetUserByAzureAdIdAsync(azureAdId);

//            if (user == null)
//            {
//                user = await _userService.GetUserByEmailAsync(email);
//            }
//            if (user == null)
//            {
//                user = new User { 
//                    Name = name ?? email, 
//                    Email = email,
//                    AzureAdId = azureAdId
//                };
//                user = await _userService.CreateUserAsync(user);
//            }

//            var response = new
//            {
//                Id = user.Id,
//                Name = user.Name,
//                Email = user.Email
//            };

//            return Ok(response);
//        }
//    }
//}


using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using mt_backend.Models;
using mt_backend.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace mt_backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class MicrosoftAuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public MicrosoftAuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("microsoft")]
        public async Task<IActionResult> MicrosoftLogin()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized("Missing or invalid token");

            var token = authHeader.Substring("Bearer ".Length);

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;

            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                return Unauthorized("Invalid token format");
            }

            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
            var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

            var azureAdId = jwtToken.Claims.FirstOrDefault(c => c.Type == "oid")?.Value ??
                jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(email))
                return Unauthorized("Email not found in token");

            if (string.IsNullOrEmpty(azureAdId))
                return Unauthorized("Azure AD ID not found in token");

            // First, try to find user by Azure AD ID
            var user = await _userService.GetUserByAzureAdIdAsync(azureAdId);

            if (user == null)
            {
                // If not found by Azure AD ID, try to find by email
                user = await _userService.GetUserByEmailAsync(email);

                if (user != null)
                {
                    // ✅ IMPORTANT: Update existing user with Azure AD ID
                    Console.WriteLine($"🔄 Updating existing user {user.Email} with Azure AD ID: {azureAdId}");
                    user.AzureAdId = azureAdId;

                    // Also update name if it's different/better
                    if (!string.IsNullOrEmpty(name) && name != user.Name)
                    {
                        user.Name = name;
                    }

                    // You'll need to add an UpdateUserAsync method to IUserService
                    await _userService.UpdateUserAsync(user);
                }
                else
                {
                    // Create new user if doesn't exist
                    Console.WriteLine($"👤 Creating new user with email: {email}, Azure AD ID: {azureAdId}");
                    user = new User
                    {
                        Name = name ?? email,
                        Email = email,
                        AzureAdId = azureAdId
                    };
                    user = await _userService.CreateUserAsync(user);
                }
            }
            else
            {
                Console.WriteLine($"✅ Found existing user by Azure AD ID: {azureAdId}");
            }

            var response = new
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AzureAdId = user.AzureAdId  // ✅ Include AzureAdId in response for debugging
            };

            return Ok(response);
        }
    }
}