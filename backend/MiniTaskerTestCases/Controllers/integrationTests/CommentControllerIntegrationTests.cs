using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using mt_backend.Data;
using mt_backend.Models;
using mt_backend.DTOs;
using mt_backend.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiniTasker.Tests.Services.IntegrationTests
{
    [TestFixture]
    public class CommentServiceIntegrationTests
    {
        private MiniTaskerDbContext _context;
        private CommentService _service;
        private TaskItem _task;
        private User _user;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MiniTaskerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MiniTaskerDbContext(options);
            _service = new CommentService(_context);

            _user = new User { Id = 1, Name = "Kasundi", Email = "kasundi@example.com", Password = "hashed" };
            _task = new TaskItem { Id = 1, Title = "Task", Description = "Task Description", DueDate = DateTime.UtcNow.AddDays(7) };

            _context.Users.Add(_user);
            _context.Tasks.Add(_task);
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Add_Then_Get_Comment_Success()
        {
            var dto = new CommentDto { UserId = _user.Id, Content = "Integration Comment" };
            await _service.AddCommentAsync(_task.Id, dto);

            var comments = await _service.GetCommentsAsync(_task.Id);
            Assert.IsTrue(comments.Any(c => c.Content == "Integration Comment"));
        }

        [Test]
        public async Task Add_Then_Update_Comment_Success()
        {
            var dto = new CommentDto { UserId = _user.Id, Content = "Initial" };
            var created = await _service.AddCommentAsync(_task.Id, dto);

            var updateDto = new CommentDto { UserId = _user.Id, Content = "Updated" };
            var updated = await _service.UpdateCommentAsync(_task.Id, created.Id, updateDto);

            Assert.AreEqual("Updated", updated.Content);
        }

        [Test]
        public async Task Add_Then_Delete_Comment_Success()
        {
            var dto = new CommentDto { UserId = _user.Id, Content = "To Delete" };
            var created = await _service.AddCommentAsync(_task.Id, dto);

            var result = await _service.DeleteCommentAsync(_task.Id, created.Id);
            Assert.IsTrue(result);

            var comments = await _service.GetCommentsAsync(_task.Id);
            Assert.IsFalse(comments.Any(c => c.Id == created.Id));
        }
    }
}
