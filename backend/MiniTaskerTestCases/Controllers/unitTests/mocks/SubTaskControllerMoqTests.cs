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