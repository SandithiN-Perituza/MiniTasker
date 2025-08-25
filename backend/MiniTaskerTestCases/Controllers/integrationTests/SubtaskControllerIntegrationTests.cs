
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
    public class SubtaskControllerIntegrationTests
    {
        private SubtaskController _controller;
        private MiniTaskerDbContext _context;
        private TaskItem _task;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MiniTaskerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MiniTaskerDbContext(options);

            _task = new TaskItem
            {
                Id = 1,
                Title = "Main Task",
                Description = "Main Task Description",
                Status = mt_backend.Models.TaskStatus.Pending,
                DueDate = DateTime.UtcNow.AddDays(7)
            };

            _context.Tasks.Add(_task);
            _context.SaveChanges();

            _controller = new SubtaskController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task CreateSubtask_AndRetrieveSubtask_Success()
        {
            var dto = new CreateSubtaskDto { Title = "Integration Subtask" };
            var createResult = await _controller.CreateSubtask(_task.Id, dto);
            Assert.IsInstanceOf<CreatedAtActionResult>(createResult.Result);

            var getResult = await _controller.GetSubtasks(_task.Id);
            Assert.IsInstanceOf<OkObjectResult>(getResult.Result);
            var subtasks = (getResult.Result as OkObjectResult).Value as IEnumerable<SubtaskDto>;
            Assert.IsTrue(subtasks.Any(s => s.Title == "Integration Subtask"));
        }

        [Test]
        public async Task MarkSubtaskCompleted_IntegrationTest()
        {
            var subtask = new Subtask { Title = "Incomplete", IsCompleted = false, TaskId = _task.Id };
            _context.Subtasks.Add(subtask);
            _context.SaveChanges();

            var result = await _controller.MarkSubtaskCompleted(_task.Id, subtask.Id);
            Assert.IsInstanceOf<OkObjectResult>(result);

            var updatedSubtask = await _context.Subtasks.FindAsync(subtask.Id);
            Assert.IsTrue(updatedSubtask.IsCompleted);
        }

        [Test]
        public async Task UpdateSubtask_IntegrationTest()
        {
            var subtask = new Subtask { Title = "Old Title", IsCompleted = false, TaskId = _task.Id };
            _context.Subtasks.Add(subtask);
            _context.SaveChanges();

            var dto = new CreateSubtaskDto { Title = "Updated Title" };
            var result = await _controller.UpdateSubtask(_task.Id, subtask.Id, dto);
            Assert.IsInstanceOf<OkObjectResult>(result);

            var updatedSubtask = await _context.Subtasks.FindAsync(subtask.Id);
            Assert.AreEqual("Updated Title", updatedSubtask.Title);
        }

        [Test]
        public async Task DeleteSubtask_IntegrationTest()
        {
            var subtask = new Subtask { Title = "To Delete", IsCompleted = false, TaskId = _task.Id };
            _context.Subtasks.Add(subtask);
            _context.SaveChanges();

            var result = await _controller.DeleteSubtask(_task.Id, subtask.Id);
            Assert.IsInstanceOf<OkObjectResult>(result);

            var deletedSubtask = await _context.Subtasks.FindAsync(subtask.Id);
            Assert.IsNull(deletedSubtask);
        }
    }
}
