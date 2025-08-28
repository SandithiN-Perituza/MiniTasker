using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using mt_backend.Controllers;
using mt_backend.Data;
using mt_backend.DTOs;
using mt_backend.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniTasker.Tests.Controllers.unitTests.mocks
{
    //[TestFixture]
    public class UsersControllerSimpleMoqTests
    {
        private UsersController _controller;

        [SetUp]
        public void Setup()
        {
            var user = new User
            {
                Id = 1,
                Name = "Kasundi",
                Email = "kasundi@example.com"
            };

            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, "1234");

            var users = new List<User> { user }.AsQueryable();

            var mockSet = new Mock<DbSet<User>>();
            mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
            mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
            mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            var mockContext = new Mock<MiniTaskerDbContext>();
            mockContext.Setup(c => c.Users).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            _controller = new UsersController(mockContext.Object);
        }

        //[Test]
        public async Task GetUsers_ReturnsAllUsers()
        {
            var result = await _controller.GetUsers();
            Assert.IsInstanceOf<ActionResult<IEnumerable<User>>>(result);
            Assert.IsNotEmpty(result.Value);
        }

        //[Test]
        public async Task CreateUser_HashesPasswordAndReturnsCreatedUser()
        {
            var newUser = new User { Name = "New", Email = "new@example.com", Password = "1234" };

            var result = await _controller.CreateUser(newUser);

            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
            var createdUser = (result.Result as CreatedAtActionResult).Value as User;
            Assert.AreEqual("New", createdUser.Name);
            Assert.AreNotEqual("1234", createdUser.Password); // Password should be hashed
        }

        //[Test]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            var request = new LoginRequest { Email = "kasundi@example.com", Password = "1234" };
            var result = await _controller.Login(request);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        //[Test]
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
