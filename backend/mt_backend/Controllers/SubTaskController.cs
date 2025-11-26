using Microsoft.AspNetCore.Mvc;
using mt_backend.DTOs;
using mt_backend.Services;
using mt_backend.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace mt_backend.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/[controller]")]
    //This protects all actions in this controller
    //[Authorize] 
    public class SubtaskController : ControllerBase
    {
        private readonly ISubtaskService _subtaskService;
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IErrorLogger _errorLogger;

        public SubtaskController(
            ISubtaskService subtaskService,
            ITaskService taskService,
            IUserService userService,
            INotificationService notificationService,
            IErrorLogger errorLogger)
        {
            _subtaskService = subtaskService;
            _taskService = taskService;
            _userService = userService;
            _notificationService = notificationService;
            _errorLogger = errorLogger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubtaskDto>>> GetSubtasks(int taskId)
        {
            var subtasks = await _subtaskService.GetSubtasksAsync(taskId);
            return Ok(subtasks);
        }

        [HttpPost]
        public async Task<ActionResult<SubtaskDto>> CreateSubtask(int taskId, CreateSubtaskDto dto)
        {
            try
            {
                await _errorLogger.LogAsync(
                    "CreateSubtask endpoint called",
                    $"TaskId: {taskId}, Title: {dto.Title}",
                    "SubtaskController.CreateSubtask"
                );

                var subtask = await _subtaskService.CreateSubtaskAsync(taskId, dto);
                if (subtask == null) return NotFound("Task not found.");

                await _errorLogger.LogAsync(
                    "Subtask created successfully",
                    $"SubtaskId: {subtask.Id}, TaskId: {taskId}",
                    "SubtaskController.CreateSubtask"
                );

                // Send notification to the assigned user of the task
                var notificationResult = await SendSubtaskNotificationToAssignedUser(taskId, subtask);

                var response = new
                {
                    message = "Subtask created Successfully",
                    subtask = subtask,
                    notification = notificationResult,
                    metadata = new
                    {
                        subtaskCreatedAt = DateTime.UtcNow,
                        notificationAttempted = true
                    }
                };

                return CreatedAtAction(nameof(GetSubtasks), new { taskId }, response);
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Error creating subtask: {ex.Message}",
                    ex.StackTrace ?? "No stack trace",
                    "SubtaskController.CreateSubtask"
                );

                Console.WriteLine($"Error creating subtask: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private async Task<NotificationResultDto> SendSubtaskNotificationToAssignedUser(int taskId, SubtaskDto subtask)
        {
            var result = new NotificationResultDto
            {
                Method = "SendSubtaskNotificationToAssignedUser",
                TokenType = "Authorization Header Token"
            };

            try
            {
                // Get the task to find the assigned user
                var task = await _taskService.GetTaskByIdAsync(taskId);
                if (task == null || task.AssignedTo == 0)
                {
                    await _errorLogger.LogAsync(
                        "Notification skipped - no assigned user",
                        $"TaskId: {taskId}, AssignedTo: {task?.AssignedTo ?? 0}",
                        "SubtaskController.SendSubtaskNotificationToAssignedUser"
                    );
                    result.Success = false;
                    result.Message = "No assigned user";
                    return result;
                }

                // Get the assigned user's Azure AD ID
                var assignedUserAzureAdId = await _userService.ResolveAzureUserId(task.AssignedTo);
                result.RecipientId = assignedUserAzureAdId;

                // Get the access token from the Authorization header
                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    await _errorLogger.LogAsync(
                        "Notification skipped - no access token available",
                        $"TaskId: {taskId}, SubtaskId: {subtask.Id}",
                        "SubtaskController.SendSubtaskNotificationToAssignedUser"
                    );

                    result.Success = false;
                    result.Message = "No access token available";
                    return result;
                }

                var token = authHeader.Substring("Bearer ".Length);

                // Extract sender information from token
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var senderName = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "Unknown User";

                result.SenderName = senderName;

                // Create notification message
                var message = $"New subtask added to your task '{task.Title}': \"{subtask.Title}\"";
                result.Message = message;

                await _errorLogger.LogAsync(
                    "Sending subtask notification to assigned user",
                    $"AssignedUserAzureAdId: {assignedUserAzureAdId}, SenderName: {senderName}, TaskId: {taskId}, SubtaskId: {subtask.Id}",
                    "SubtaskController.SendSubtaskNotificationToAssignedUser"
                );

                await _notificationService.SendNotificationWithOBOTokenAsync(
                    recipientAzureAdId: assignedUserAzureAdId,
                    message: message,
                    userAccessToken: token,
                    senderName: senderName
                );

                await _errorLogger.LogAsync(
                    "Subtask notification sent successfully",
                    $"Notification sent to {assignedUserAzureAdId} for subtask {subtask.Id} on task {taskId}",
                    "SubtaskController.SendSubtaskNotificationToAssignedUser"
                );

                result.Success = true;
                result.Message = $"Notification sent successfully via OBO to {assignedUserAzureAdId}";
                return result;
            }
            catch (Exception notificationEx)
            {
                await _errorLogger.LogAsync(
                    $"Failed to send subtask notification: {notificationEx.Message}",
                    notificationEx.StackTrace ?? "No stack trace",
                    "SubtaskController.SendSubtaskNotificationToAssignedUser"
                );

                Console.WriteLine($"⚠️ SUBTASK NOTIFICATION FAILED: {notificationEx.Message}");

                result.Success = false;
                result.ErrorDetails = notificationEx.Message;
                return result;
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> MarkSubtaskCompleted(int taskId, int id)
        {
            var subtask = await _subtaskService.MarkSubtaskCompletedAsync(taskId, id);
            if (subtask == null) return NotFound("Subtask not found.");

            return Ok(new { message = "SubTask Updated Successfully", subtask });
        }

        [HttpPatch("{id}/mark-incomplete")]
        public async Task<IActionResult> MarkSubtaskIncomplete(int taskId, int id)
        {
            var subtask = await _subtaskService.MarkSubtaskIncompleteAsync(taskId, id);
            if (subtask == null) return NotFound("Subtask not found.");

            return Ok(new { message = "SubTask marked as incomplete successfully", subtask });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubtask(int taskId, int id, CreateSubtaskDto dto)
        {
            var subtask = await _subtaskService.UpdateSubtaskAsync(taskId, id, dto);
            if (subtask == null) return NotFound("Subtask not found.");

            return Ok(new { message = "SubTask Updated Successfully", subtask });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubtask(int taskId, int id)
        {
            var success = await _subtaskService.DeleteSubtaskAsync(taskId, id);
            if (!success) return NotFound("Subtask not found.");

            return Ok(new { message = "SubTask deleted Successfully" });
        }
    }
}