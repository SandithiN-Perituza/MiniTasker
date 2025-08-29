using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using mt_backend.Controllers;
using mt_backend.Data;
using mt_backend.Models;
using mt_backend.DTOs;
using mt_backend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniTasker.Tests.Controllers.integrationTests
{
    [TestFixture]
    public class UsersControllerIntegrationTests
    {
        private UsersController _controller;
        private MiniTaskerDbContext _context;
        private IUserService _userService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MiniTaskerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MiniTaskerDbContext(options);
            _userService = new UserService(_context);
            _controller = new UsersController(_userService);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task CreateUser_AndRetrieveUser_Success()
        {
            var request = new CreateUserRequestDto
            {
                Name = "Kasundi",
                Email = "kasundi@example.com",
                Password = "1234"
            };

            var result = await _controller.CreateUser(request);
            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);

            var getResult = await _controller.GetUsers();

            var okResult = getResult.Result as OkObjectResult;

            Assert.IsNotNull(okResult, "Expected OkObjectResult but got null.");
            var users = okResult.Value as IEnumerable<UserResponseDto>;

            Assert.IsNotNull(users, "Expected user list but got null.");
            Assert.IsTrue(users.Any(u => u.Email == "kasundi@example.com"), "User not found in retrieved list.");

        }

        [Test]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            var user = new User
            {
                Name = "Kasundi",
                Email = "kasundi@example.com",
                Password = "1234"
            };

            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, user.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            var loginRequest = new LoginRequest
            {
                Email = "kasundi@example.com",
                Password = "1234"
            };

            var result = await _controller.Login(loginRequest);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Login_InvalidEmail_ReturnsUnauthorized()
        {
            var loginRequest = new LoginRequest
            {
                Email = "wrong@example.com",
                Password = "1234"
            };

            var result = await _controller.Login(loginRequest);
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }
    }
}
