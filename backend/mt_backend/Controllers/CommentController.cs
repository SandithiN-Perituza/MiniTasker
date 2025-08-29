using Microsoft.AspNetCore.Mvc;
using mt_backend.DTOs;
using mt_backend.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mt_backend.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
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
            var comment = await _commentService.AddCommentAsync(taskId, dto);
            if (comment == null)
                return NotFound("Task or User not found.");

            return Ok(new { message = "Comment created Successfully", comment });
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
