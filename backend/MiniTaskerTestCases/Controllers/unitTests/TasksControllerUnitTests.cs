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

namespace MiniTasker.Tests.Controllers.UnitTests
{
    [TestFixture]
    public class TasksControllerUnitTests
    {
        private TasksController _controller;
        private MiniTaskerDbContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MiniTaskerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MiniTaskerDbContext(options);

            var user = new User { Id = 1, Name = "Kasundi", Email = "kasundi@example.com", Password = "hashed" };
            _context.Users.Add(user);

            var task = new TaskItem
            {
                Id = 1,
                Title = "Test Task",
                Description = "Test Description",
                Status = TaskStatus.Pending,
                AssignedTo = 1,
                DueDate = DateTime.UtcNow.AddDays(7),
                AssignedUser = user
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();

            var taskService = new TaskService(_context);
            _controller = new TasksController(taskService);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task GetTasks_ReturnsAllTasks()
        {
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
                Title = "New Task",
                Description = "New Description",
                Status = TaskStatus.Pending,
                AssignedTo = 1,
                DueDate = DateTime.UtcNow.AddDays(5)
            };

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

            var result = await _controller.UpdateTask(1, updatedTask);
            Assert.IsInstanceOf<OkObjectResult>(result);
            var task = (result as OkObjectResult).Value as TaskItem;
            Assert.AreEqual("Updated Task", task.Title);
        }

        [Test]
        public async Task DeleteTask_ValidId_DeletesTask()
        {
            var result = await _controller.DeleteTask(1);
            Assert.IsInstanceOf<OkObjectResult>(result);
            var message = (result as OkObjectResult).Value.ToString();
            Assert.IsTrue(message.Contains("deleted successfully"));
        }

        [Test]
        public async Task GetTaskById_ValidId_ReturnsTask()
        {
            var result = await _controller.GetTaskById(1);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var task = (result.Result as OkObjectResult).Value as TaskItemDto;
            Assert.AreEqual(1, task.Id);
        }
    }
}
