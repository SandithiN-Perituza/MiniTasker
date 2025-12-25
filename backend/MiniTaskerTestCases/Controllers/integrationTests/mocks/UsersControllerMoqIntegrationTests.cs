//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using mt_backend.Controllers;
//using mt_backend.Data;
//using mt_backend.DTOs;
//using mt_backend.Models;
//using NUnit.Framework;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace MiniTasker.Tests.Controllers.integrationTests.mocks
//{
//    //[TestFixture]
//    public class UsersControllerSimpleMoqTests
//    {

//    }
//}

using Microsoft.AspNetCore.Mvc;
using Moq;
using mt_backend.Controllers;
using mt_backend.DTOs;
using mt_backend.Models;
using mt_backend.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniTasker.Tests.Controllers.integrationTests.mocks
{
    [TestFixture]
    public class UsersControllerSimpleMoqTests
    {
        private Mock<IUserService> _mockUserService;
        private UsersController _controller;

        [SetUp]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UsersController(_mockUserService.Object);
        }

        [Test]
        public async Task GetUsers_ReturnsListOfUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Name = "Alice", Email = "alice@example.com" },
                new User { Id = 2, Name = "Bob", Email = "bob@example.com" }
            };
            _mockUserService.Setup(s => s.GetUsersAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsers();
            var okResult = result.Result as OkObjectResult;
            var returnedUsers = okResult?.Value as IEnumerable<UserResponseDto>;

            // Assert
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(2, returnedUsers?.Count());
        }

        [Test]
        public async Task CreateUser_ReturnsCreatedUser()
        {
            // Arrange
            var request = new CreateUserRequestDto
            {
                Name = "Charlie",
                Email = "charlie@example.com",
                Password = "securepassword"
            };
            var createdUser = new User
            {
                Id = 3,
                Name = request.Name,
                Email = request.Email,
                Password = "hashedpassword"
            };
            _mockUserService.Setup(s => s.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(createdUser);

            // Act
            var result = await _controller.CreateUser(request);
            var createdAtResult = result.Result as CreatedAtActionResult;
            var responseDto = createdAtResult?.Value as UserResponseDto;

            // Assert
            Assert.IsNotNull(createdAtResult);
            Assert.AreEqual(201, createdAtResult.StatusCode);
            Assert.AreEqual(request.Name, responseDto?.Name);
            Assert.AreEqual(request.Email, responseDto?.Email);
        }

        [Test]
        public async Task Login_WithValidCredentials_ReturnsUser()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "dave@example.com",
                Password = "password123"
            };
            var user = new User
            {
                Id = 4,
                Name = "Dave",
                Email = loginRequest.Email
            };
            _mockUserService.Setup(s => s.LoginAsync(loginRequest)).ReturnsAsync(user);

            // Act
            var result = await _controller.Login(loginRequest);
            var okResult = result as OkObjectResult;
            var responseDto = okResult?.Value as UserResponseDto;

            // Assert
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(user.Name, responseDto?.Name);
            Assert.AreEqual(user.Email, responseDto?.Email);
        }

        [Test]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "invalid@example.com",
                Password = "wrongpassword"
            };
            _mockUserService.Setup(s => s.LoginAsync(loginRequest)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.Login(loginRequest);
            var unauthorizedResult = result as UnauthorizedObjectResult;

            // Assert
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
            Assert.AreEqual("Invalid email or password.", unauthorizedResult.Value);
        }
    }
}