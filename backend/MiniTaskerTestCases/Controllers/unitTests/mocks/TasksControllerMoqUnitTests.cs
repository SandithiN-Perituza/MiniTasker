using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Moq;
using mt_backend.Controllers;
using mt_backend.Models;
using mt_backend.DTOs;
using mt_backend.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskStatus = mt_backend.Models.TaskStatus;

namespace MiniTasker.Tests.Controllers.unitTests
{
    [TestFixture]
    public class TasksControllerMoqUnitTests
    {
        private TasksController _controller;
        private Mock<ITaskService> _mockTaskService;

        [SetUp]
        public void Setup()
        {
            _mockTaskService = new Mock<ITaskService>();
            _controller = new TasksController(_mockTaskService.Object);
        }

        [Test]
        public async Task GetTasks_ReturnsAllTasks()
        {
            var mockTasks = new List<TaskItemDto>
            {
                new TaskItemDto { Id = 1, Title = "Test Task", Description = "Test Desc", Status = "Pending", AssignedTo = 1, AssignedUserName = "Kasundi", DueDate = DateTime.UtcNow.AddDays(7) }
            };

            _mockTaskService.Setup(s => s.GetTasksAsync()).ReturnsAsync(mockTasks);

            var result = await _controller.GetTasks();

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var tasks = (result.Result as OkObjectResult).Value as IEnumerable<TaskItemDto>;
            Assert.IsNotEmpty(tasks);
        }

        [Test]
        public async Task CreateTask_AddsNewTask()
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
            var createdTask = (result.Result as CreatedAtActionResult).Value as TaskItem;
            Assert.AreEqual("New Task", createdTask.Title);
        }

        [Test]
        public async Task UpdateTask_ValidId_UpdatesTask()
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
            var task = (result as OkObjectResult).Value as TaskItem;
            Assert.AreEqual("Updated Task", task.Title);
        }

        [Test]
        public async Task DeleteTask_ValidId_DeletesTask()
        {
            _mockTaskService.Setup(s => s.DeleteTaskAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteTask(1);

            Assert.IsInstanceOf<OkObjectResult>(result);
            var message = (result as OkObjectResult).Value.ToString();
            Assert.IsTrue(message.Contains("deleted successfully"));
        }

        [Test]
        public async Task GetTaskById_ValidId_ReturnsTask()
        {
            var taskDto = new TaskItemDto
            {
                Id = 1,
                Title = "Test Task",
                Description = "Test Description",
                Status = "Pending",
                AssignedTo = 1,
                AssignedUserName = "Kasundi",
                DueDate = DateTime.UtcNow.AddDays(7)
            };

            _mockTaskService.Setup(s => s.GetTaskByIdAsync(1)).ReturnsAsync(taskDto);

            var result = await _controller.GetTaskById(1);

            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var task = (result.Result as OkObjectResult).Value as TaskItemDto;
            Assert.AreEqual(1, task.Id);
        }
    }
}
