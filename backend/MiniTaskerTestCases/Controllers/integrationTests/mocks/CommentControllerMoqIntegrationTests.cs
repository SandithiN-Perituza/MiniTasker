using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using mt_backend.Data;
using mt_backend.Models;
using mt_backend.DTOs;
using mt_backend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniTasker.Tests.Services.IntegrationTests
{
    [TestFixture]
    public class CommentServiceMoqIntegrationTests
    {
        private MiniTaskerDbContext context;
        private CommentService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MiniTaskerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            context = new MiniTaskerDbContext(options);

            // Seed data
            var user = new User { Id = 1, Name = "Kasundi", Email = "kasundi@example.com", Password = "hashed" };
            var task = new TaskItem { Id = 1, Title = "Task 1" };

            context.Users.Add(user);
            context.Tasks.Add(task);
            context.SaveChanges();

            _service = new CommentService(context);
        }

        [TearDown]
        public void TearDown()
        {
            context.Dispose();
        }

        [Test]
        public async Task AddCommentAsync_ValidData_AddsComment()
        {
            var dto = new CommentDto { UserId = 1, Content = "Test Comment" };

            var result = await _service.AddCommentAsync(1, dto);

            Assert.IsNotNull(result);
            Assert.AreEqual("Test Comment", result.Content);
        }

        [Test]
        public async Task GetCommentsAsync_ReturnsAllComments()
        {
            context.Comments.Add(new Comment
            {
                Id = 1,
                TaskId = 1,
                UserId = 1,
                Content = "Comment 1",
                CreatedAt = DateTime.UtcNow,
                User = context.Users.First()
            });
            context.SaveChanges();

            var result = await _service.GetCommentsAsync(1);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(c => c.Content == "Comment 1"));
        }
    }
}
