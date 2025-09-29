using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly INotificationService _notifier;
        private readonly IErrorLogger _errorLogger;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, INotificationService notifier, IErrorLogger errorLogger ,ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _notifier = notifier;
            _errorLogger = errorLogger;
            _logger = logger;
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
                Console.WriteLine($"Error in GetTasks: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving tasks.", error = ex.Message });
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
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
        {
            var createdTask = await _taskService.CreateTaskAsync(task);

            try
            {
                var assignedUser = await _taskService.GetTaskUserByIdAsync(task.AssignedTo ?? 0);

                if (assignedUser != null && !string.IsNullOrEmpty(assignedUser.AzureAdId))
                {
                    var taskUrl = $"https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net/?highlight={createdTask.Id}";

                    await _notifier.SendTaskCreatedNotificationAsync(
                        assignedUser.AzureAdId,
                        createdTask.Id.ToString(),
                        "MiniTasker Bot",
                        taskUrl
                    );
                }
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(ex.Message, ex.StackTrace ?? "No stack trace", "TasksController.CreateTask");
            }

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
