
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

namespace MiniTasker.Tests.Controllers.integrationTests
{
    [TestFixture]
    public class CommentControllerIntegrationTests
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
        public async Task AddComment_AndRetrieveComment_Success()
        {
            var dto = new CommentDto { UserId = _user.Id, Content = "Integration Comment" };
            var addResult = await _controller.AddComment(_task.Id, dto);
            Assert.IsInstanceOf<OkObjectResult>(addResult.Result);

            var getResult = await _controller.GetComments(_task.Id);
            Assert.IsInstanceOf<OkObjectResult>(getResult.Result);
            var comments = (getResult.Result as OkObjectResult).Value as IEnumerable<CommentResponseDto>;
            Assert.IsTrue(comments.Any(c => c.Content == "Integration Comment"));
        }

        [Test]
        public async Task UpdateComment_IntegrationTest()
        {
            var comment = new Comment { TaskId = _task.Id, UserId = _user.Id, Content = "Old", CreatedAt = DateTime.UtcNow };
            _context.Comments.Add(comment);
            _context.SaveChanges();

            var dto = new CommentDto { UserId = _user.Id, Content = "Updated" };
            var updateResult = await _controller.UpdateComment(_task.Id, comment.Id, dto);
            Assert.IsInstanceOf<OkObjectResult>(updateResult);

            var updatedComment = await _context.Comments.FindAsync(comment.Id);
            Assert.AreEqual("Updated", updatedComment.Content);
        }

        [Test]
        public async Task DeleteComment_IntegrationTest()
        {
            var comment = new Comment { TaskId = _task.Id, UserId = _user.Id, Content = "To Delete", CreatedAt = DateTime.UtcNow };
            _context.Comments.Add(comment);
            _context.SaveChanges();

            var deleteResult = await _controller.DeleteComment(_task.Id, comment.Id);
            Assert.IsInstanceOf<OkObjectResult>(deleteResult);

            var deletedComment = await _context.Comments.FindAsync(comment.Id);
            Assert.IsNull(deletedComment);
        }
    }
}
