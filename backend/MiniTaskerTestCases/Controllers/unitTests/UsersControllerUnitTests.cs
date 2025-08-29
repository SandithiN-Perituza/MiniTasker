using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mt_backend.Controllers;
using mt_backend.Data;
using mt_backend.DTOs;
using mt_backend.Models;
using mt_backend.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniTasker.Tests.Controllers.unitTests
{
    [TestFixture]
    public class UsersControllerUnitTests
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

            var user = new User
            {
                Id = 1,
                Name = "Kasundi",
                Email = "kasundi@example.com",
                Password = "1234"
            };

            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, user.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            _userService = new UserService(_context);
            _controller = new UsersController(_userService);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task GetUsers_ReturnsAllUsers()
        {
            var result = await _controller.GetUsers();

            // Extract the actual result from ActionResult
            var okResult = result.Result as OkObjectResult;

            Assert.IsNotNull(okResult, "Expected OkObjectResult but got null.");

            var users = okResult.Value as IEnumerable<UserResponseDto>;

            Assert.IsNotNull(users, "Expected user list but got null.");
            Assert.IsTrue(users.Any(), "Expected at least one user in the result.");

        }

        [Test]
        public async Task CreateUser_HashesPasswordAndReturnsCreatedUser()
        {
            var newUserRequest = new CreateUserRequestDto
            {
                Name = "New",
                Email = "new@example.com",
                Password = "1234"
            };

            var result = await _controller.CreateUser(newUserRequest);

            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
            var createdUser = (result.Result as CreatedAtActionResult).Value as UserResponseDto;
            Assert.AreEqual("New", createdUser.Name);
            Assert.AreEqual("new@example.com", createdUser.Email);
        }

        [Test]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            var request = new LoginRequest { Email = "kasundi@example.com", Password = "1234" };
            var result = await _controller.Login(request);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
        {
            var request = new LoginRequest { Email = "wrong@example.com", Password = "1234" };
            var result = await _controller.Login(request);

            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual(401, unauthorizedResult?.StatusCode);
        }
    }
}
