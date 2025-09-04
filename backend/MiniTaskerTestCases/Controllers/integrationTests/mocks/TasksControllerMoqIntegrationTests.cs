using Microsoft.AspNetCore.Mvc;
using Moq;
using mt_backend.Controllers;
using mt_backend.DTOs;
using mt_backend.Models;
using mt_backend.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskStatus = mt_backend.Models.TaskStatus;

namespace MiniTasker.Tests.Controllers.integrationTests.Mocks
{
    [TestFixture]
    public class TasksControllerMoqIntegrationTests
    {
        private Mock<ITaskService> _mockTaskService;
        private TasksController _controller;

        [SetUp]
        public void Setup()
        {
            _mockTaskService = new Mock<ITaskService>();
            _controller = new TasksController(_mockTaskService.Object);
        }

        [Test]
        public async Task GetTasks_ReturnsOkWithTasks()
        {
            var mockTasks = new List<TaskItemDto>
            {
                new TaskItemDto { Id = 1, Title = "Mock Task", Description = "Mock Desc", Status = "Pending", AssignedTo = 1, AssignedUserName = "Kasundi" }
            };

            _mockTaskService.Setup(s => s.GetTasksAsync()).ReturnsAsync(mockTasks);

            var result = await _controller.GetTasks();
            Assert.IsInstanceOf<OkObjectResult>(result.Result);

            var okResult = result.Result as OkObjectResult;
            var tasks = okResult.Value as IEnumerable<TaskItemDto>;

            Assert.IsNotNull(tasks);
            Assert.AreEqual(1, tasks.Count());
        }

        [Test]
        public async Task GetTaskById_ValidId_ReturnsOk()
        {
            var taskDto = new TaskItemDto
            {
                Id = 1,
                Title = "Find Me",
                Description = "Find Description",
                Status = "Pending",
                AssignedTo = 1,
                AssignedUserName = "Kasundi"
            };

            _mockTaskService.Setup(s => s.GetTaskByIdAsync(1)).ReturnsAsync(taskDto);

            var result = await _controller.GetTaskById(1);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);

            var okResult = result.Result as OkObjectResult;
            var task = okResult.Value as TaskItemDto;

            Assert.AreEqual("Find Me", task.Title);
        }

        [Test]
        public async Task CreateTask_ReturnsCreatedAtAction()
        {
            var newTask = new TaskItem
            {
                Id = 2,
                Title = "New Task",
                Description = "New Description",
                Status = TaskStatus.Pending,
                AssignedTo = 1,
                DueDate = DateTime.UtcNow.AddDays(5)
            };

            _mockTaskService.Setup(s => s.CreateTaskAsync(It.IsAny<TaskItem>())).ReturnsAsync(newTask);

            var result = await _controller.CreateTask(newTask);
            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);

            var created = (result.Result as CreatedAtActionResult).Value as TaskItem;
            Assert.AreEqual("New Task", created.Title);
        }

        [Test]
        public async Task UpdateTask_ValidId_ReturnsOk()
        {
            var updatedTask = new TaskItem
            {
                Id = 1,
                Title = "Updated Task",
                Description = "Updated Description",
                Status = TaskStatus.Complete,
                AssignedTo = 1,
                DueDate = DateTime.UtcNow.AddDays(10)
            };

            _mockTaskService.Setup(s => s.UpdateTaskAsync(1, updatedTask)).ReturnsAsync(updatedTask);

            var result = await _controller.UpdateTask(1, updatedTask);
            Assert.IsInstanceOf<OkObjectResult>(result);

            var okResult = result as OkObjectResult;
            var task = okResult.Value as TaskItem;

            Assert.AreEqual("Updated Task", task.Title);
        }

        [Test]
        public async Task DeleteTask_ValidId_ReturnsOk()
        {
            _mockTaskService.Setup(s => s.DeleteTaskAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteTask(1);
            Assert.IsInstanceOf<OkObjectResult>(result);

            var okResult = result as OkObjectResult;
            Assert.IsTrue(okResult.Value.ToString().Contains("deleted successfully"));
        }

        //[Test]
        //public async Task DeleteTask_InvalidId_ReturnsNotFound()
        //{
        //    _mockTaskService.Setup(s => s.DeleteTaskAsync(999)).ReturnsAsync(false);

        //    var result = await _controller.DeleteTask(999);
        //    Assert.IsInstanceOf<NotFoundResult>(result);
        //}


    }
}
