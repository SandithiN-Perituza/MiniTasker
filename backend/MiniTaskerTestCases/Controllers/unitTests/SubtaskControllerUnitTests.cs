
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

namespace MiniTasker.Tests.Controllers.unitTests
{
    [TestFixture]
    public class SubtaskControllerUnitTests
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
        public async Task GetSubtasks_ReturnsSubtasks()
        {
            _context.Subtasks.Add(new Subtask { Title = "Subtask 1", IsCompleted = false, TaskId = _task.Id });
            _context.SaveChanges();

            var result = await _controller.GetSubtasks(_task.Id);
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var subtasks = (result.Result as OkObjectResult).Value as IEnumerable<SubtaskDto>;
            Assert.IsNotEmpty(subtasks);
        }

        [Test]
        public async Task CreateSubtask_ValidTaskId_CreatesSubtask()
        {
            var dto = new CreateSubtaskDto { Title = "New Subtask" };
            var result = await _controller.CreateSubtask(_task.Id, dto);

            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
            var subtask = (result.Result as CreatedAtActionResult).Value as SubtaskDto;
            Assert.AreEqual("New Subtask", subtask.Title);
        }

        [Test]
        public async Task MarkSubtaskCompleted_ValidId_UpdatesStatus()
        {
            var subtask = new Subtask { Title = "Subtask", IsCompleted = false, TaskId = _task.Id };
            _context.Subtasks.Add(subtask);
            _context.SaveChanges();

            var result = await _controller.MarkSubtaskCompleted(_task.Id, subtask.Id);
            Assert.IsInstanceOf<OkObjectResult>(result);
            var response = (result as OkObjectResult).Value;
            Assert.IsTrue(subtask.IsCompleted);
        }

        [Test]
        public async Task UpdateSubtask_ValidId_UpdatesTitle()
        {
            var subtask = new Subtask { Title = "Old Title", IsCompleted = false, TaskId = _task.Id };
            _context.Subtasks.Add(subtask);
            _context.SaveChanges();

            var dto = new CreateSubtaskDto { Title = "Updated Title" };
            var result = await _controller.UpdateSubtask(_task.Id, subtask.Id, dto);

            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.AreEqual("Updated Title", subtask.Title);
        }

        [Test]
        public async Task DeleteSubtask_ValidId_DeletesSubtask()
        {
            var subtask = new Subtask { Title = "To Delete", IsCompleted = false, TaskId = _task.Id };
            _context.Subtasks.Add(subtask);
            _context.SaveChanges();

            var result = await _controller.DeleteSubtask(_task.Id, subtask.Id);
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.IsNull(await _context.Subtasks.FindAsync(subtask.Id));
        }
    }
}
