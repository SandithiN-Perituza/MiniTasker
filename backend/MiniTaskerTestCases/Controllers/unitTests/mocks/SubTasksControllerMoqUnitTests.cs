//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using mt_backend.Controllers;
//using mt_backend.Data;
//using mt_backend.DTOs;
//using mt_backend.Models;
//using NUnit.Framework;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace MiniTasker.Tests.Controllers.unitTests.mocks
//{
//    //[TestFixture]
//    public class SubTasksControllerMoqUnitTests
//    {

//    }
//}


//using NUnit.Framework;
//using Moq;
//using Microsoft.EntityFrameworkCore;
//using mt_backend.Controllers;
//using mt_backend.Data;
//using mt_backend.Models;
//using mt_backend.DTOs;
//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace MiniTasker.Tests
//{
//    public class SubtaskControllerTests
//    {
//        private Mock<MiniTaskerDbContext> _mockContext;
//        private Mock<DbSet<TaskItem>> _mockTaskSet;
//        private Mock<DbSet<Subtask>> _mockSubtaskSet;
//        private SubtaskController _controller;

//        [SetUp]
//        public void Setup()
//        {
//            // Seed TaskItem
//            var tasks = new List<TaskItem>
//            {
//                new TaskItem { Id = 1, Title = "Main Task" }
//            };

//            var subtasks = new List<Subtask>();

//            _mockTaskSet = CreateMockDbSet(tasks);
//            _mockSubtaskSet = CreateMockDbSet(subtasks);

//            _mockContext = new Mock<MiniTaskerDbContext>();
//            _mockContext.Setup(c => c.Tasks).Returns(_mockTaskSet.Object);
//            _mockContext.Setup(c => c.Subtasks).Returns(_mockSubtaskSet.Object);

//            // Mock FindAsync for Tasks
//            _mockContext.Setup(c => c.Tasks.FindAsync(It.IsAny<object[]>()))
//                .ReturnsAsync((object[] ids) => tasks.FirstOrDefault(t => t.Id == (int)ids[0]));

//            // Mock SaveChangesAsync
//            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
//                .ReturnsAsync(1);

//            _controller = new SubtaskController(_mockContext.Object);
//        }

//        [Test]
//        public async Task CreateSubtask_ReturnsCreatedSubtaskDto()
//        {
//            // Arrange
//            var dto = new CreateSubtaskDto { Title = "New Subtask" };

//            // Act
//            var result = await _controller.CreateSubtask(1, dto);

//            // Assert
//            var createdResult = result.Result as CreatedAtActionResult;
//            Assert.IsNotNull(createdResult);

//            var subtaskDto = createdResult.Value as SubtaskDto;
//            Assert.IsNotNull(subtaskDto);
//            Assert.AreEqual("New Subtask", subtaskDto.Title);
//            Assert.IsFalse(subtaskDto.IsCompleted);
//        }

//        // Helper method to mock DbSet<T>
//        private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> sourceList) where T : class
//        {
//            var queryable = sourceList.AsQueryable();
//            var mockSet = new Mock<DbSet<T>>();

//            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
//            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
//            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
//            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

//            mockSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(sourceList.Add);

//            return mockSet;
//        }
//    }
//}

using Microsoft.AspNetCore.Mvc;
using Moq;
using mt_backend.Controllers;
using mt_backend.DTOs;
using mt_backend.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniTasker.Tests.Controllers.unitTests.mocks
{
    [TestFixture]
    public class SubTasksControllerMoqUnitTests
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
        public async Task GetSubtasks_ReturnsListOfSubtasks()
        {
            var taskId = 1;
            var subtasks = new List<SubtaskDto>
    {
        new SubtaskDto { Id = 1, Title = "Subtask 1", IsCompleted = false },
        new SubtaskDto { Id = 2, Title = "Subtask 2", IsCompleted = true }
    };

            _mockService.Setup(s => s.GetSubtasksAsync(taskId)).ReturnsAsync(subtasks);

            var result = await _controller.GetSubtasks(taskId);
            var okResult = result.Result as OkObjectResult;

            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(subtasks, okResult.Value);
        }

        [Test]
        public async Task CreateSubtask_ReturnsCreatedSubtask()
        {
            var taskId = 1;
            var dto = new CreateSubtaskDto { Title = "New Subtask" };
            var createdSubtask = new SubtaskDto { Id = 3, Title = "New Subtask", IsCompleted = false };

            _mockService.Setup(s => s.CreateSubtaskAsync(taskId, dto)).ReturnsAsync(createdSubtask);

            var result = await _controller.CreateSubtask(taskId, dto);
            var createdResult = result.Result as CreatedAtActionResult;

            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(createdSubtask, createdResult.Value);
        }

        [Test]
        public async Task MarkSubtaskCompleted_ReturnsUpdatedSubtask()
        {
            var taskId = 1;
            var subtaskId = 3;
            var updatedSubtask = new SubtaskDto { Id = subtaskId, Title = "Subtask", IsCompleted = true };

            _mockService.Setup(s => s.MarkSubtaskCompletedAsync(taskId, subtaskId)).ReturnsAsync(updatedSubtask);

            var result = await _controller.MarkSubtaskCompleted(taskId, subtaskId) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task MarkSubtaskIncomplete_ReturnsUpdatedSubtask()
        {
            var taskId = 1;
            var subtaskId = 3;
            var updatedSubtask = new SubtaskDto { Id = subtaskId, Title = "Subtask", IsCompleted = false };

            _mockService.Setup(s => s.MarkSubtaskIncompleteAsync(taskId, subtaskId)).ReturnsAsync(updatedSubtask);

            var result = await _controller.MarkSubtaskIncomplete(taskId, subtaskId) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task UpdateSubtask_ReturnsUpdatedSubtask()
        {
            var taskId = 1;
            var subtaskId = 3;
            var dto = new CreateSubtaskDto { Title = "Updated Title" };
            var updatedSubtask = new SubtaskDto { Id = subtaskId, Title = "Updated Title", IsCompleted = false };

            _mockService.Setup(s => s.UpdateSubtaskAsync(taskId, subtaskId, dto)).ReturnsAsync(updatedSubtask);

            var result = await _controller.UpdateSubtask(taskId, subtaskId, dto) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task DeleteSubtask_ReturnsSuccessMessage()
        {
            var taskId = 1;
            var subtaskId = 3;

            _mockService.Setup(s => s.DeleteSubtaskAsync(taskId, subtaskId)).ReturnsAsync(true);

            var result = await _controller.DeleteSubtask(taskId, subtaskId) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }
    }
}
