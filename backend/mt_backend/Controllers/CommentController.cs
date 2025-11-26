using Microsoft.AspNetCore.Mvc;
using mt_backend.DTOs;
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
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IErrorLogger _errorLogger;

        public CommentController(
            ICommentService commentService,
            ITaskService taskService,
            IUserService userService,
            INotificationService notificationService,
            IErrorLogger errorLogger)
        {
            _commentService = commentService;
            _taskService = taskService;
            _userService = userService;
            _notificationService = notificationService;
            _errorLogger = errorLogger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetComments(int taskId)
        {
            var comments = await _commentService.GetCommentsAsync(taskId);
            return Ok(comments);
        }

        [HttpPost]
        public async Task<ActionResult<CommentResponseDto>> AddComment(int taskId, [FromBody] CommentDto dto)
        {
            try
            {
                // Add validation
                if (dto == null)
                {
                    return BadRequest(new { error = "Comment data is required" });
                }

                if (string.IsNullOrWhiteSpace(dto.Content))
                {
                    return BadRequest(new { error = "Comment content is required" });
                }

                if (dto.UserId <= 0)
                {
                    return BadRequest(new { error = "Valid user ID is required" });
                }

                await _errorLogger.LogAsync(
                    "AddComment endpoint called",
                    $"TaskId: {taskId}, UserId: {dto.UserId}, Content: {dto.Content}",
                    "CommentController.AddComment"
                );

                var comment = await _commentService.AddCommentAsync(taskId, dto);
                if (comment == null)
                    return NotFound("Task or User not found.");

                await _errorLogger.LogAsync(
                    "Comment created successfully",
                    $"CommentId: {comment.Id}, TaskId: {taskId}",
                    "CommentController.AddComment"
                );

                // Send notification to the assigned user of the task
                var notificationResult = await SendCommentNotificationToAssignedUser(taskId, comment, dto.UserId);

                var response = new
                {
                    message = "Comment created Successfully",
                    comment = comment,
                    notification = notificationResult,
                    metadata = new
                    {
                        commentCreatedAt = DateTime.UtcNow,
                        notificationAttempted = true
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Error adding comment: {ex.Message}",
                    ex.StackTrace ?? "No stack trace",
                    "CommentController.AddComment"
                );

                Console.WriteLine($"Error adding comment: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private async Task<NotificationResultDto> SendCommentNotificationToAssignedUser(int taskId, CommentResponseDto comment, int commenterUserId)
        {
            var result = new NotificationResultDto
            {
                Method = "SendCommentNotificationToAssignedUser",
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
                        "CommentController.SendCommentNotificationToAssignedUser"
                    );
                    result.Success = false;
                    result.Message = "No assigned user";
                    return result;
                }

                // Don't notify if the commenter is the same as the assigned user
                if (task.AssignedTo == commenterUserId)
                {
                    await _errorLogger.LogAsync(
                        "Notification skipped - commenter is the assigned user",
                        $"TaskId: {taskId}, AssignedUserId: {task.AssignedTo}",
                        "CommentController.SendCommentNotificationToAssignedUser"
                    );
                    result.Success = false;
                    result.Message = "Commenter is the assigned user";
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
                        $"TaskId: {taskId}, CommentId: {comment.Id}",
                        "CommentController.SendCommentNotificationToAssignedUser"
                    );

                    result.Success = false;
                    result.Message = "No access token available";
                    return result;
                }

                var token = authHeader.Substring("Bearer ".Length);

                // Get commenter information
                var commenterUser = await _userService.GetUserByIdAsync(commenterUserId);
                var commenterName = commenterUser?.Name ?? "Unknown User";

                // Extract sender information from token as fallback
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    var tokenUserName = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                    if (!string.IsNullOrEmpty(tokenUserName))
                    {
                        commenterName = tokenUserName;
                    }
                }
                catch (Exception ex)
                {
                    await _errorLogger.LogAsync(
                        $"Failed to extract user name from token: {ex.Message}",
                        "Using commenter name from database",
                        "CommentController.SendCommentNotificationToAssignedUser"
                    );
                }

                result.SenderName = commenterName;

                // Create notification message
                var message = $"New comment added to your task '{task.Title}': \"{comment.Content}\"";
                result.Message = message;

                await _errorLogger.LogAsync(
                    "Sending comment notification to assigned user",
                    $"AssignedUserAzureAdId: {assignedUserAzureAdId}, CommenterName: {commenterName}, TaskId: {taskId}, CommentId: {comment.Id}",
                    "CommentController.SendCommentNotificationToAssignedUser"
                );

                await _notificationService.SendNotificationWithOBOTokenAsync(
                    recipientAzureAdId: assignedUserAzureAdId,
                    message: message,
                    userAccessToken: token,
                    senderName: commenterName
                );

                await _errorLogger.LogAsync(
                    "Comment notification sent successfully",
                    $"Notification sent to {assignedUserAzureAdId} for comment {comment.Id} on task {taskId}",
                    "CommentController.SendCommentNotificationToAssignedUser"
                );

                result.Success = true;
                result.Message = $"Notification sent successfully via OBO to {assignedUserAzureAdId}";
                return result;
            }
            catch (Exception notificationEx)
            {
                await _errorLogger.LogAsync(
                    $"Failed to send comment notification: {notificationEx.Message}",
                    notificationEx.StackTrace ?? "No stack trace",
                    "CommentController.SendCommentNotificationToAssignedUser"
                );

                Console.WriteLine($"⚠️ COMMENT NOTIFICATION FAILED: {notificationEx.Message}");

                result.Success = false;
                result.ErrorDetails = notificationEx.Message;
                return result;
            }
        }

        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(int taskId, int commentId, [FromBody] CommentDto dto)
        {
            var updated = await _commentService.UpdateCommentAsync(taskId, commentId, dto);
            if (updated == null)
                return NotFound("Comment not found.");

            return Ok(new { message = "Comment Updated Successfully", comment = updated });
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int taskId, int commentId)
        {
            var success = await _commentService.DeleteCommentAsync(taskId, commentId);
            if (!success)
                return NotFound("Comment not found.");

            return Ok("Comment Deleted Successfully");
        }
    }
}