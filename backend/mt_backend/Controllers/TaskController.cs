using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mt_backend.DTOs;
using mt_backend.Models;
using mt_backend.Services.Interfaces;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace mt_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IErrorLogger _errorLogger;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public TasksController(ITaskService taskService, IErrorLogger errorLogger, IUserService userService, INotificationService notificationService)
        {
            _taskService = taskService;
            _errorLogger = errorLogger;
            _userService = userService;
            _notificationService = notificationService;
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
                Console.WriteLine(ex);
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

        //[HttpPost]
        //public async Task<ActionResult<TaskItem>> CreateTask(CreateTaskRequestDto dto)
        //{
        //    try
        //    {
        //        await _errorLogger.LogAsync(
        //            "CreateTask endpoint called",
        //            $"Title: {dto.Title}, AssignedTo: {dto.AssignedTo}, ActorName: {dto.ActorName}",
        //            "TasksController.CreateTask"
        //        );

        //        var task = new TaskItem
        //        {
        //            Title = dto.Title,
        //            Description = dto.Description,
        //            AssignedTo = dto.AssignedTo,
        //            DueDate = dto.DueDate
        //        };

        //        var createdTask = await _taskService.CreateTaskAsync(task, dto.ActorName ?? "Unknown");

        //        await _errorLogger.LogAsync(
        //            "Task created successfully",
        //            $"TaskId: {createdTask.Id}, Title: {createdTask.Title}",
        //            "TasksController.CreateTask"
        //        );

        //        // Send notification to assigned user
        //        await SendNotificationToAssignedUser(createdTask, dto.ActorName);

        //        return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _errorLogger.LogAsync(
        //            $"Error creating task: {ex.Message}",
        //            ex.StackTrace ?? "No stack trace",
        //            "TasksController.CreateTask"
        //        );

        //        Console.WriteLine($"Error creating task: {ex.Message}");
        //        return StatusCode(500, new { error = ex.Message });
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask([FromBody] object requestObject)
        {
            try
            {
                await _errorLogger.LogAsync(
                    "CreateTask endpoint called",
                    $"Request received: {System.Text.Json.JsonSerializer.Serialize(requestObject)}",
                    "TasksController.CreateTask"
                );

                // Try to determine if this is a simple or complex request
                var requestJson = System.Text.Json.JsonSerializer.Serialize(requestObject);
                var requestDocument = System.Text.Json.JsonDocument.Parse(requestJson);
                var root = requestDocument.RootElement;

                TaskItem task;
                string? actorName = null;
                bool isNotificationRequest = false;
                CreateTaskWithNotificationRequestDto? notificationRequest = null;

                // Check if this is a notification request (has task property)
                if (root.TryGetProperty("task", out _))
                {
                    // This is a notification request from frontend
                    notificationRequest = System.Text.Json.JsonSerializer.Deserialize<CreateTaskWithNotificationRequestDto>(requestJson, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                    
                });

                    if (notificationRequest?.Task == null)
                    {
                        return BadRequest(new { error = "Invalid task data in notification request" });
                    }

                    task = new TaskItem
                    {
                        Title = notificationRequest.Task.Title,
                        Description = notificationRequest.Task.Description,
                        AssignedTo = notificationRequest.Task.AssignedTo,
                        DueDate = notificationRequest.Task.DueDate
                    };

                    actorName = notificationRequest.UserName ?? "Unknown";
                    isNotificationRequest = true;

                    await _errorLogger.LogAsync(
                        "Parsed notification request",
                        $"Title: {task.Title}, AssignedTo: {task.AssignedTo}, ActorName: {actorName}, AssignedUserAzureAdId: {notificationRequest.AssignedUserAzureAdId}",
                        "TasksController.CreateTask"
                    );
                }
                else
                {
                    // This is a simple request
                    var simpleRequest = System.Text.Json.JsonSerializer.Deserialize<CreateTaskRequestDto>(requestJson, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (simpleRequest == null)
                    {
                        return BadRequest(new { error = "Invalid task request" });
                    }

                    task = new TaskItem
                    {
                        Title = simpleRequest.Title,
                        Description = simpleRequest.Description,
                        AssignedTo = simpleRequest.AssignedTo,
                        DueDate = simpleRequest.DueDate
                    };

                    actorName = simpleRequest.ActorName ?? "Unknown";

                    await _errorLogger.LogAsync(
                        "Parsed simple request",
                        $"Title: {task.Title}, AssignedTo: {task.AssignedTo}, ActorName: {actorName}",
                        "TasksController.CreateTask"
                    );
                }

                var createdTask = await _taskService.CreateTaskAsync(task, actorName);

                await _errorLogger.LogAsync(
                    "Task created successfully",
                    $"TaskId: {createdTask.Id}, Title: {createdTask.Title}",
                    "TasksController.CreateTask"
                );

                // Handle notifications
                if (isNotificationRequest && notificationRequest != null)
                {
                    // Use the provided information from frontend
                    await SendNotificationWithProvidedInfo(createdTask, notificationRequest);
                }
                else
                {
                    // Fallback to database lookup method
                    await SendNotificationToAssignedUser(createdTask, actorName);
                }

                return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Error creating task: {ex.Message}",
                    ex.StackTrace ?? "No stack trace",
                    "TasksController.CreateTask"
                );

                Console.WriteLine($"Error creating task: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
        private async Task SendNotificationToAssignedUser(TaskItem task, string? actorName)
        {
            try
            {
                if (task.AssignedTo == null)
                {
                    await _errorLogger.LogAsync(
                        "Notification skipped - no assigned user",
                        $"TaskId: {task.Id}",
                        "TasksController.SendNotificationToAssignedUser"
                    );
                    return;
                }

                // Get the assigned user's Azure AD ID from the database
                var assignedUserAzureAdId = await _userService.ResolveAzureUserId(task.AssignedTo.Value);

                // Get the access token from the Authorization header
                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    await _errorLogger.LogAsync(
                        "Notification skipped - no access token available",
                        $"TaskId: {task.Id}, AssignedUserId: {task.AssignedTo}",
                        "TasksController.SendNotificationToAssignedUser"
                    );
                    Console.WriteLine($"📝 NOTIFICATION SKIPPED: No access token available for task {task.Id}");
                    return;
                }

                var token = authHeader.Substring("Bearer ".Length);

                // Extract sender information from token
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var senderName = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value ??
                               actorName ??
                               "Unknown User";

                // Create notification message
                var message = $"You have been assigned a new task: '{task.Title}'";

                await _errorLogger.LogAsync(
                    "Sending notification to assigned user",
                    $"AssignedUserAzureAdId: {assignedUserAzureAdId}, SenderName: {senderName}, TaskId: {task.Id}",
                    "TasksController.SendNotificationToAssignedUser"
                );

                // Send notification using the delegated permissions method
                await _notificationService.SendNotificationWithTokenAsync(
                    recipientAzureAdId: assignedUserAzureAdId,
                    message: message,
                    accessToken: token,
                    senderName: senderName
                );

                await _errorLogger.LogAsync(
                    "Notification sent successfully",
                    $"Notification sent to {assignedUserAzureAdId} for task {task.Id}",
                    "TasksController.SendNotificationToAssignedUser"
                );
            }
            catch (Exception notificationEx)
            {
                await _errorLogger.LogAsync(
                    $"Failed to send notification but task created successfully: {notificationEx.Message}",
                    notificationEx.StackTrace ?? "No stack trace",
                    "TasksController.SendNotificationToAssignedUser"
                );

                // Don't fail the entire request if notification fails
                Console.WriteLine($"⚠️ NOTIFICATION FAILED: {notificationEx.Message}");
                Console.WriteLine($"✅ TASK CREATED: Task {task.Id} created successfully despite notification failure");
            }
        }

        private async Task SendNotificationWithProvidedInfo(TaskItem task, CreateTaskWithNotificationRequestDto request)
        {
            try
            {
                await _errorLogger.LogAsync(
                    "SendNotificationWithProvidedInfo called",
                    $"TaskId: {task.Id}, AssignedUserAzureAdId: '{request.AssignedUserAzureAdId}', IsNull: {string.IsNullOrEmpty(request.AssignedUserAzureAdId)}",
                    "TasksController.SendNotificationWithProvidedInfo"
                );
                if (string.IsNullOrEmpty(request.AssignedUserAzureAdId))
                {
                    await _errorLogger.LogAsync(
                        "Notification skipped - no assigned user Azure AD ID provided",
                        $"TaskId: {task.Id}",
                        "TasksController.SendNotificationWithProvidedInfo"
                    );
                    return;
                }

                // Use Graph token if available, otherwise fall back to auth token
                var accessToken = !string.IsNullOrEmpty(request.GraphToken) ? request.GraphToken : request.AuthToken;

                if (string.IsNullOrEmpty(accessToken))
                {
                    // Try to get token from Authorization header as fallback
                    var authHeader = Request.Headers["Authorization"].ToString();
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    {
                        accessToken = authHeader.Substring("Bearer ".Length);
                    }
                }

                if (string.IsNullOrEmpty(accessToken))
                {
                    await _errorLogger.LogAsync(
                        "Notification skipped - no access token available",
                        $"TaskId: {task.Id}, AssignedUserAzureAdId: {request.AssignedUserAzureAdId}",
                        "TasksController.SendNotificationWithProvidedInfo"
                    );
                    Console.WriteLine($"📝 NOTIFICATION SKIPPED: No access token available for task {task.Id}");
                    return;
                }

                var senderName = request.UserName ?? "Unknown User";
                var message = request.Message ?? $"You have been assigned a new task: '{task.Title}'";

                await _errorLogger.LogAsync(
                    "Sending notification with provided information",
                    $"AssignedUserAzureAdId: {request.AssignedUserAzureAdId}, SenderName: {senderName}, TaskId: {task.Id}, UsingGraphToken: {!string.IsNullOrEmpty(request.GraphToken)}",
                    "TasksController.SendNotificationWithProvidedInfo"
                );

                // Send notification using the delegated permissions method
                await _notificationService.SendNotificationWithTokenAsync(
                    recipientAzureAdId: request.AssignedUserAzureAdId,
                    message: message,
                    accessToken: accessToken,
                    senderName: senderName
                );

                await _errorLogger.LogAsync(
                    "Notification sent successfully with provided info",
                    $"Notification sent to {request.AssignedUserAzureAdId} for task {task.Id}",
                    "TasksController.SendNotificationWithProvidedInfo"
                );
            }
            catch (Exception notificationEx)
            {
                await _errorLogger.LogAsync(
                    $"Failed to send notification with provided info: {notificationEx.Message}",
                    notificationEx.StackTrace ?? "No stack trace",
                    "TasksController.SendNotificationWithProvidedInfo"
                );

                // Don't fail the entire request if notification fails
                Console.WriteLine($"⚠️ NOTIFICATION FAILED: {notificationEx.Message}");
                Console.WriteLine($"✅ TASK CREATED: Task {task.Id} created successfully despite notification failure");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem updatedTask)
        {
            try
            {
                if (id != updatedTask.Id) return BadRequest(new { message = "Task ID mismatch" });

                var task = await _taskService.UpdateTaskAsync(id, updatedTask);

                if (task == null) return NotFound(new { message = $"Task with ID {id} not found." });

                return Ok(task);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating task: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var success = await _taskService.DeleteTaskAsync(id);
                if (!success) return NotFound(new { message = "Task deletion was not successful" });

                return Ok(new { message = $"Task with ID {id} deleted successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting task: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}