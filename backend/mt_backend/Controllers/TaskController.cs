using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using mt_backend.DTOs;
using mt_backend.Models;
using mt_backend.Services;
using mt_backend.Services.Interfaces;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace mt_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //This protects all actions in this controller
    //[Authorize] 
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly INotificationService _notifier;
        private readonly IErrorLogger _errorLogger;
        private readonly IUserService _userService;

        public TasksController(ITaskService taskService, IErrorLogger errorLogger, IUserService userService, INotificationService notifier)
        {
            _taskService = taskService;
            _notifier = notifier;
            _errorLogger = errorLogger;
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasks()
        {
            try
            {
                var tasks = await _taskService.GetTasksAsync();
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); // Or use your logger
                return StatusCode(500, new { error = ex.ToString() });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetMyTasks()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token." });

            var tasks = await _taskService.GetTasksForUserAsync(userId);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItemDto>> GetTaskById(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound(new { message = "Task with ID is not found" });
            return Ok(task);
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(CreateTaskRequestDto dto)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                AssignedTo = dto.AssignedTo,
                DueDate = dto.DueDate
            };

            var createdTask = await _taskService.CreateTaskAsync(task, dto.ActorName);

            //try
            //{
            //    if (createdTask.AssignedTo.HasValue)
            //    {
            //        // ✅ Resolve Azure AD user ID from internal user ID
            //        var assignedUser = await _userService.GetUserByIdAsync(createdTask.AssignedTo.Value);
            //        if (assignedUser?.AzureAdId != null)
            //        {
            //            string azureUserId = assignedUser.AzureAdId;

            //            // Send activity feed notification
            //            await _notifier.SendTaskCreatedNotificationAsync(
            //                userId: azureUserId,
            //                taskId: createdTask.Id.ToString(),
            //                actorName: dto.ActorName,
            //                taskUrl: null // optional, will be built inside the service
            //            );
            //        }
            //    }
            //    else
            //    {
            //        await _errorLogger.LogAsync(
            //            "AssignedTo is null",
            //            "Cannot send notification because AssignedTo is null.",
            //            "TasksController.CreateTask"
            //        );
            //    }
            //}
            //catch (ServiceException ex)
            //{
            //    await _errorLogger.LogAsync(
            //        $"Graph API Error: {ex.StatusCode}",
            //        $"Message: {ex.Message}\nDetails: {ex.Error?.Message}",
            //        "NotificationService.SendTaskCreatedNotificationAsync"
            //    );
            //}
            //catch (Exception ex)
            //{
            //    await _errorLogger.LogAsync(
            //        $"Notification Error: {ex.Message}",
            //        ex.StackTrace ?? "No stack trace",
            //        "TasksController.CreateTask"
            //    );
            //}

            return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem updatedTask)
        {
            if (id != updatedTask.Id) return BadRequest(new { message = "Task with ID is not found" });

            var task = await _taskService.UpdateTaskAsync(id, updatedTask);

            if (task == null) return NotFound(new { message = $"Task with ID {id} not found." });

            return Ok(task);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var deletedTask = await _taskService.GetTaskByIdAsync(id);
            var success = await _taskService.DeleteTaskAsync(id);
            if (!success) return NotFound(new { message = "Task deletion with ID is not successful" });

            return Ok(new { message = $"Task with ID {id} is deleted successfully" });
        }
    }
}
