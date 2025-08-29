using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using mt_backend.Controllers;
using mt_backend.DTOs;
using mt_backend.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniTasker.Tests.Controllers.UnitTests
{
    [TestFixture]
    public class SubtaskControllerUnitTests
    {
        private Mock<ISubtaskService> _mockService;
        private SubtaskController _controller;

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<ISubtaskService>();
            _controller = new SubtaskController(_mockService.Object);
        }

        [Test]
        public async Task GetSubtasks_ReturnsOkWithSubtasks()
        {
            var subtasks = new List<SubtaskDto>
            {
                new SubtaskDto { Id = 1, Title = "Subtask 1", IsCompleted = false }
            };

            _mockService.Setup(s => s.GetSubtasksAsync(1)).ReturnsAsync(subtasks);

            var result = await _controller.GetSubtasks(1);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var value = (result.Result as OkObjectResult).Value as IEnumerable<SubtaskDto>;
            Assert.AreEqual(1, value.Count());
        }

        [Test]
        public async Task CreateSubtask_ReturnsCreatedAtAction()
        {
            var dto = new CreateSubtaskDto { Title = "New Subtask" };
            var created = new SubtaskDto { Id = 1, Title = "New Subtask", IsCompleted = false };

            _mockService.Setup(s => s.CreateSubtaskAsync(1, dto)).ReturnsAsync(created);

            var result = await _controller.CreateSubtask(1, dto);
            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
        }

        [Test]
        public async Task MarkSubtaskCompleted_ReturnsOk()
        {
            var updated = new SubtaskDto { Id = 1, Title = "Done", IsCompleted = true };

            _mockService.Setup(s => s.MarkSubtaskCompletedAsync(1, 1)).ReturnsAsync(updated);

            var result = await _controller.MarkSubtaskCompleted(1, 1);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task UpdateSubtask_ReturnsOk()
        {
            var dto = new CreateSubtaskDto { Title = "Updated" };
            var updated = new SubtaskDto { Id = 1, Title = "Updated", IsCompleted = false };

            _mockService.Setup(s => s.UpdateSubtaskAsync(1, 1, dto)).ReturnsAsync(updated);

            var result = await _controller.UpdateSubtask(1, 1, dto);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task DeleteSubtask_ReturnsOk()
        {
            _mockService.Setup(s => s.DeleteSubtaskAsync(1, 1)).ReturnsAsync(true);

            var result = await _controller.DeleteSubtask(1, 1);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
    }
}
