using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mt_backend.Controllers;
using mt_backend.Data;
using mt_backend.Models;
using mt_backend.DTOs;
using mt_backend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskStatus = mt_backend.Models.TaskStatus;

namespace MiniTasker.Tests.Controllers.IntegrationTests
{
    [TestFixture]
    public class TasksControllerIntegrationTests
    {
        private TasksController _controller;
        private MiniTaskerDbContext _context;
        private ITaskService _taskService;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MiniTaskerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MiniTaskerDbContext(options);
            _taskService = new TaskService(_context);
            _controller = new TasksController(_taskService);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task CreateTask_AndRetrieveTask_Success()
        {
            var task = new TaskItem
            {
                Title = "Integration Task",
                Description = "Task Description",
                Status = TaskStatus.Pending,
                DueDate = DateTime.UtcNow.AddDays(5)
            };

            var createResult = await _controller.CreateTask(task);
            Assert.IsInstanceOf<CreatedAtActionResult>(createResult.Result);

            var getResult = await _controller.GetTasks();
            Assert.IsInstanceOf<OkObjectResult>(getResult.Result);
            var tasks = (getResult.Result as OkObjectResult).Value as IEnumerable<TaskItemDto>;
            Assert.IsTrue(tasks.Any(t => t.Title == "Integration Task"));
        }

        [Test]
        public async Task UpdateTask_IntegrationTest()
        {
            var task = new TaskItem
            {
                Title = "Old Title",
                Description = "Old Description",
                Status = TaskStatus.Pending,
                DueDate = DateTime.UtcNow.AddDays(5)
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();

            var updatedTask = new TaskItem
            {
                Id = task.Id,
                Title = "Updated Title",
                Description = "Updated Description",
                Status = TaskStatus.Complete,
                DueDate = DateTime.UtcNow.AddDays(10)
            };

            var updateResult = await _controller.UpdateTask(task.Id, updatedTask);
            Assert.IsInstanceOf<OkObjectResult>(updateResult);

            var taskFromDb = await _context.Tasks.FindAsync(task.Id);
            Assert.AreEqual("Updated Title", taskFromDb.Title);
        }

        [Test]
        public async Task DeleteTask_IntegrationTest()
        {
            var task = new TaskItem
            {
                Title = "To Delete",
                Description = "Delete Description",
                Status = TaskStatus.Pending,
                DueDate = DateTime.UtcNow.AddDays(5)
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();

            var deleteResult = await _controller.DeleteTask(task.Id);
            Assert.IsInstanceOf<OkObjectResult>(deleteResult);

            var deletedTask = await _context.Tasks.FindAsync(task.Id);
            Assert.IsNull(deletedTask);
        }

        [Test]
        public async Task GetTaskById_IntegrationTest()
        {
            var task = new TaskItem
            {
                Title = "Find Me",
                Description = "Find Description",
                Status = TaskStatus.Pending,
                DueDate = DateTime.UtcNow.AddDays(5)
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();

            var getResult = await _controller.GetTaskById(task.Id);
            Assert.IsInstanceOf<OkObjectResult>(getResult.Result);
            var taskDto = (getResult.Result as OkObjectResult).Value as TaskItemDto;
            Assert.AreEqual("Find Me", taskDto.Title);
        }
    }
}
