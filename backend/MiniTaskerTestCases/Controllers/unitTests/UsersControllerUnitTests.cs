
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using mt_backend.Controllers;
using mt_backend.Data;
using mt_backend.DTOs;
using mt_backend.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MiniTasker.Tests.Controllers.unitTests
{
    [TestFixture]
    public class UsersControllerUnitTests
    {
        private UsersController _controller;
        private MiniTaskerDbContext _context;

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

            _controller = new UsersController(_context);
        }
        //Prevents resource leaks in long test runs.
        //Ensures test isolation — each test starts fresh.
        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task GetUsers_ReturnsAllUsers()
        {
            var result = await _controller.GetUsers();
            Assert.IsInstanceOf<ActionResult<IEnumerable<User>>>(result);
            Assert.IsNotEmpty(result.Value);
        }

        [Test]
        public async Task CreateUser_HashesPasswordAndReturnsCreatedUser()
        {
            var newUser = new User { Name = "New", Email = "new@example.com", Password = "1234" };
            var result = await _controller.CreateUser(newUser);

            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
            var createdUser = (result.Result as CreatedAtActionResult).Value as User;
            Assert.AreEqual("New", createdUser.Name);
            Assert.AreNotEqual("1234", createdUser.Password); // Password should be hashed
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
            // Arrange: Use a unique in-memory database to ensure isolation
            var options = new DbContextOptionsBuilder<MiniTaskerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new MiniTaskerDbContext(options);

            // Add a valid user to the database
            var validUser = new User
            {
                Name = "Kasundi",
                Email = "kasundi@example.com",
                Password = "1234"
            };

            var hasher = new PasswordHasher<User>();
            validUser.Password = hasher.HashPassword(validUser, validUser.Password);

            context.Users.Add(validUser);
            context.SaveChanges();

            var controller = new UsersController(context);

            // Act: Attempt login with an invalid email
            var request = new LoginRequest { Email = "wrong@example.com", Password = "1234" };
            var result = await controller.Login(request);

            // Assert: Should return Unauthorized
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.AreEqual(401, unauthorizedResult?.StatusCode);
        }
    }
}
