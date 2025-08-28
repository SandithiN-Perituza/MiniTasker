
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mt_backend.Controllers;
using mt_backend.Data;
using mt_backend.Models;
using mt_backend.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniTasker.Tests.Controllers.unitTests
{
    [TestFixture]
    public class CommentControllerUnitTests
    {
        private CommentController _controller;
        private MiniTaskerDbContext _context;
        private TaskItem _task;
        private User _user;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MiniTaskerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MiniTaskerDbContext(options);

            _user = new User { Id = 1, Name = "Kasundi", Email = "kasundi@example.com", Password = "hashed" };
            _task = new TaskItem { Id = 1, Title = "Task", Description = "Task Description", DueDate = DateTime.UtcNow.AddDays(7) };

            _context.Users.Add(_user);
            _context.Tasks.Add(_task);
            _context.SaveChanges();

            _controller = new CommentController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task GetComments_ReturnsComments()
        {
            _context.Comments.Add(new Comment { TaskId = _task.Id, UserId = _user.Id, Content = "Test Comment", CreatedAt = DateTime.UtcNow });
            _context.SaveChanges();

            var result = await _controller.GetComments(_task.Id);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var comments = (result.Result as OkObjectResult).Value as IEnumerable<CommentResponseDto>;
            Assert.IsNotEmpty(comments);
        }

        [Test]
        public async Task AddComment_ValidData_CreatesComment()
        {
            var dto = new CommentDto { UserId = _user.Id, Content = "New Comment" };
            var result = await _controller.AddComment(_task.Id, dto);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var response = (result.Result as OkObjectResult).Value;
            Assert.IsNotNull(response);
        }

        [Test]
        public async Task UpdateComment_ValidId_UpdatesContent()
        {
            var comment = new Comment { TaskId = _task.Id, UserId = _user.Id, Content = "Old Content", CreatedAt = DateTime.UtcNow };
            _context.Comments.Add(comment);
            _context.SaveChanges();

            var dto = new CommentDto { UserId = _user.Id, Content = "Updated Content" };
            var result = await _controller.UpdateComment(_task.Id, comment.Id, dto);

            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.AreEqual("Updated Content", comment.Content);
        }

        [Test]
        public async Task DeleteComment_ValidId_DeletesComment()
        {
            var comment = new Comment { TaskId = _task.Id, UserId = _user.Id, Content = "To Delete", CreatedAt = DateTime.UtcNow };
            _context.Comments.Add(comment);
            _context.SaveChanges();

            var result = await _controller.DeleteComment(_task.Id, comment.Id);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsNull(await _context.Comments.FindAsync(comment.Id));
        }
    }
}
