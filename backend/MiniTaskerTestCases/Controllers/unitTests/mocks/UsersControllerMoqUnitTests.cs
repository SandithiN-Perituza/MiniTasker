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

namespace mt_backend.Tests
{
    [TestFixture]
    public class UsersControllerMoqUnitTests
    {
        private Mock<IUserService> _mockUserService;
        private UsersController _controller;

        [SetUp]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>();

            var users = new List<User>
            {
                new User { Id = 1, Name = "Kasundi", Email = "kasundi@example.com", Password = "hashed1" },
                new User { Id = 2, Name = "Senanayake", Email = "senanayake@example.com", Password = "hashed2" }
            };

            _mockUserService.Setup(s => s.GetUsersAsync()).ReturnsAsync(users);

            _controller = new UsersController(_mockUserService.Object);
        }

        [Test]
        public async Task GetUsers_ReturnsAllUsers()
        {
            var result = await _controller.GetUsers();

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult, "Expected OkObjectResult but got null.");

            var users = okResult.Value as IEnumerable<UserResponseDto>;
            Assert.IsNotNull(users, "Expected user list but got null.");
            Assert.IsTrue(users.Any(), "Expected at least one user in the result.");
        }



        [Test]
        public async Task CreateUser_AddsUserWithHashedPassword()
        {
            var request = new CreateUserRequestDto
            {
                Name = "New User",
                Email = "new@example.com",
                Password = "plainpassword"
            };

            var createdUser = new User
            {
                Id = 3,
                Name = request.Name,
                Email = request.Email,
                Password = "hashedpassword"
            };

            _mockUserService.Setup(s => s.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(createdUser);

            var result = await _controller.CreateUser(request);

            var createdResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            var response = createdResult.Value as UserResponseDto;
            Assert.IsNotNull(response);
            Assert.AreEqual("New User", response.Name);
            Assert.AreEqual("new@example.com", response.Email);
        }

        [Test]
        public async Task Login_ReturnsOk_WhenCredentialsAreValid()
        {
            var request = new LoginRequest
            {
                Email = "login@example.com",
                Password = "securepassword"
            };

            var user = new User
            {
                Id = 4,
                Name = "Login User",
                Email = request.Email,
                Password = "hashedpassword"
            };

            _mockUserService.Setup(s => s.LoginAsync(request)).ReturnsAsync(user);

            var result = await _controller.Login(request);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
        {
            var request = new LoginRequest
            {
                Email = "unknown@example.com",
                Password = "password"
            };

            _mockUserService.Setup(s => s.LoginAsync(request)).ReturnsAsync((User)null);

            var result = await _controller.Login(request);

            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }

        [Test]
        public async Task Login_ReturnsUnauthorized_WhenPasswordIsIncorrect()
        {
            var request = new LoginRequest
            {
                Email = "wrongpass@example.com",
                Password = "wrongpassword"
            };

            _mockUserService.Setup(s => s.LoginAsync(request)).ReturnsAsync((User)null);

            var result = await _controller.Login(request);

            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }
    }
}
