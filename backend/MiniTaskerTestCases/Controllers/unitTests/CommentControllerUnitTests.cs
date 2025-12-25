using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using mt_backend.Data;
using mt_backend.Models;
using mt_backend.DTOs;
using mt_backend.Services;
using System;
using System.Threading.Tasks;

namespace MiniTasker.Tests.Services.UnitTests
{
    [TestFixture]
    public class CommentServiceUnitTests
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
        public async Task AddCommentAsync_ValidData_ReturnsCommentResponse()
        {
            var dto = new CommentDto { UserId = _user.Id, Content = "Test Comment" };

            var result = await _service.AddCommentAsync(_task.Id, dto);

            Assert.IsNotNull(result);
            Assert.AreEqual("Test Comment", result.Content);
        }

        [Test]
        public async Task GetCommentsAsync_ReturnsAllComments()
        {
            _context.Comments.Add(new Comment
            {
                TaskId = _task.Id,
                UserId = _user.Id,
                Content = "Comment 1",
                CreatedAt = DateTime.UtcNow
            });
            _context.SaveChanges();

            var result = await _service.GetCommentsAsync(_task.Id);

            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public async Task UpdateCommentAsync_ValidId_UpdatesContent()
        {
            var comment = new Comment
            {
                TaskId = _task.Id,
                UserId = _user.Id,
                Content = "Old Content",
                CreatedAt = DateTime.UtcNow
            };
            _context.Comments.Add(comment);
            _context.SaveChanges();

            var dto = new CommentDto { UserId = _user.Id, Content = "Updated Content" };
            var result = await _service.UpdateCommentAsync(_task.Id, comment.Id, dto);

            Assert.IsNotNull(result);
            Assert.AreEqual("Updated Content", result.Content);
        }

        [Test]
        public async Task DeleteCommentAsync_ValidId_DeletesComment()
        {
            var comment = new Comment
            {
                TaskId = _task.Id,
                UserId = _user.Id,
                Content = "To Delete",
                CreatedAt = DateTime.UtcNow
            };
            _context.Comments.Add(comment);
            _context.SaveChanges();

            var result = await _service.DeleteCommentAsync(_task.Id, comment.Id);

            Assert.IsTrue(result);
            Assert.IsNull(await _context.Comments.FindAsync(comment.Id));
        }
    }
}
