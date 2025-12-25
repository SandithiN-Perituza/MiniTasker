using Microsoft.AspNetCore.Mvc;
using Moq;
using mt_backend.Controllers;
using mt_backend.DTOs;
using mt_backend.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniTasker.Tests.Controllers.integrationTests.Mocks
{
    [TestFixture]
    public class SubTasksControllerMoqIntegrationTests
    {
        private SubtaskController _controller;
        private Mock<ISubtaskService> _mockService;

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<ISubtaskService>();
            _controller = new SubtaskController(_mockService.Object);
        }

        [Test]
        public async Task GetSubtasks_ReturnsSubtasks()
        {
            var taskId = 1;
            var subtasks = new List<SubtaskDto>
            {
                new SubtaskDto { Id = 1, Title = "Subtask 1", IsCompleted = false },
                new SubtaskDto { Id = 2, Title = "Subtask 2", IsCompleted = true }
            };

            _mockService.Setup(s => s.GetSubtasksAsync(taskId)).ReturnsAsync(subtasks);

            var result = await _controller.GetSubtasks(taskId);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var returned = (result.Result as OkObjectResult).Value as IEnumerable<SubtaskDto>;
            Assert.AreEqual(2, returned.Count());
        }

        [Test]
        public async Task CreateSubtask_ValidTask_ReturnsCreatedSubtask()
        {
            var taskId = 1;
            var dto = new CreateSubtaskDto { Title = "New Subtask" };
            var created = new SubtaskDto { Id = 3, Title = "New Subtask", IsCompleted = false };

            _mockService.Setup(s => s.CreateSubtaskAsync(taskId, dto)).ReturnsAsync(created);

            var result = await _controller.CreateSubtask(taskId, dto);
            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
            var returned = (result.Result as CreatedAtActionResult).Value as SubtaskDto;
            Assert.AreEqual("New Subtask", returned.Title);
        }

        [Test]
        public async Task CreateSubtask_TaskNotFound_ReturnsNotFound()
        {
            var taskId = 99;
            var dto = new CreateSubtaskDto { Title = "Invalid" };

            _mockService.Setup(s => s.CreateSubtaskAsync(taskId, dto)).ReturnsAsync((SubtaskDto?)null);

            var result = await _controller.CreateSubtask(taskId, dto);
            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
        }

        [Test]
        public async Task MarkSubtaskCompleted_Valid_ReturnsUpdatedSubtask()
        {
            var taskId = 1;
            var subtaskId = 2;
            var updated = new SubtaskDto { Id = subtaskId, Title = "Done", IsCompleted = true };

            _mockService.Setup(s => s.MarkSubtaskCompletedAsync(taskId, subtaskId)).ReturnsAsync(updated);

            var result = await _controller.MarkSubtaskCompleted(taskId, subtaskId);
            Assert.IsInstanceOf<OkObjectResult>(result);
            var response = (result as OkObjectResult).Value;
            Assert.IsTrue(response.ToString().Contains("SubTask Updated Successfully"));
        }

        [Test]
        public async Task UpdateSubtask_Valid_ReturnsUpdatedSubtask()
        {
            var taskId = 1;
            var subtaskId = 2;
            var dto = new CreateSubtaskDto { Title = "Updated Title" };
            var updated = new SubtaskDto { Id = subtaskId, Title = "Updated Title", IsCompleted = false };

            _mockService.Setup(s => s.UpdateSubtaskAsync(taskId, subtaskId, dto)).ReturnsAsync(updated);

            var result = await _controller.UpdateSubtask(taskId, subtaskId, dto);
            Assert.IsInstanceOf<OkObjectResult>(result);
            var response = (result as OkObjectResult).Value;
            Assert.IsTrue(response.ToString().Contains("SubTask Updated Successfully"));
        }

        [Test]
        public async Task DeleteSubtask_Valid_ReturnsSuccessMessage()
        {
            var taskId = 1;
            var subtaskId = 2;

            _mockService.Setup(s => s.DeleteSubtaskAsync(taskId, subtaskId)).ReturnsAsync(true);

            var result = await _controller.DeleteSubtask(taskId, subtaskId);
            Assert.IsInstanceOf<OkObjectResult>(result);
            var message = (result as OkObjectResult).Value.ToString();
            Assert.IsTrue(message.Contains("deleted Successfully"));
        }

        [Test]
        public async Task DeleteSubtask_NotFound_ReturnsNotFound()
        {
            var taskId = 1;
            var subtaskId = 999;

            _mockService.Setup(s => s.DeleteSubtaskAsync(taskId, subtaskId)).ReturnsAsync(false);

            var result = await _controller.DeleteSubtask(taskId, subtaskId);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }
    }
}
