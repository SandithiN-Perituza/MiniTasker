using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using mt_backend.Controllers;
using mt_backend.DTOs;
using mt_backend.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniTasker.Tests.Controllers.UnitTests
{
    [TestFixture]
    public class CommentControllerMoqUnitTests
    {
        private Mock<ICommentService> _mockService;
        private CommentController _controller;

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<ICommentService>();
            _controller = new CommentController(_mockService.Object);
        }

        [Test]
        public async Task GetComments_ReturnsComments()
        {
            var taskId = 1;
            var comments = new List<CommentResponseDto>
            {
                new CommentResponseDto { Id = 1, TaskId = taskId, UserId = 1, UserName = "Kasundi", Content = "Test", CreatedAt = DateTime.UtcNow }
            };

            _mockService.Setup(s => s.GetCommentsAsync(taskId)).ReturnsAsync(comments);

            var result = await _controller.GetComments(taskId);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var returned = (result.Result as OkObjectResult).Value as IEnumerable<CommentResponseDto>;
            Assert.IsNotEmpty(returned);
        }

        [Test]
        public async Task AddComment_ValidData_ReturnsCreatedComment()
        {
            var taskId = 1;
            var dto = new CommentDto { UserId = 1, Content = "New Comment" };
            var response = new CommentResponseDto
            {
                Id = 1,
                TaskId = taskId,
                UserId = 1,
                UserName = "Kasundi",
                Content = "New Comment",
                CreatedAt = DateTime.UtcNow
            };

            _mockService.Setup(s => s.AddCommentAsync(taskId, dto)).ReturnsAsync(response);

            var result = await _controller.AddComment(taskId, dto);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public async Task UpdateComment_ValidId_UpdatesContent()
        {
            var taskId = 1;
            var commentId = 1;
            var dto = new CommentDto { UserId = 1, Content = "Updated" };
            var updated = new CommentResponseDto
            {
                Id = commentId,
                TaskId = taskId,
                UserId = 1,
                UserName = "Kasundi",
                Content = "Updated",
                CreatedAt = DateTime.UtcNow
            };

            _mockService.Setup(s => s.UpdateCommentAsync(taskId, commentId, dto)).ReturnsAsync(updated);

            var result = await _controller.UpdateComment(taskId, commentId, dto);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task DeleteComment_ValidId_DeletesComment()
        {
            var taskId = 1;
            var commentId = 1;

            _mockService.Setup(s => s.DeleteCommentAsync(taskId, commentId)).ReturnsAsync(true);

            var result = await _controller.DeleteComment(taskId, commentId);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
    }
}
