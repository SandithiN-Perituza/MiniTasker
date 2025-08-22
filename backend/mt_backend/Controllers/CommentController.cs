using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mt_backend.Data;
using mt_backend.DTOs;
using mt_backend.Models;

namespace mt_backend.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly MiniTaskerDbContext _context;

        public CommentController(MiniTaskerDbContext context)
        {
            _context = context;
        }

        // GET: api/tasks/{taskId}/comment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(int taskId)
        {
            var comments = await _context.Comments
                .Where(c => c.TaskId == taskId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentResponseDto
                {
                    Id = c.Id,
                    TaskId = c.TaskId,
                    UserId = c.UserId,
                    UserName = c.User.Name,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(comments);
        }

        // POST: api/tasks/{taskId}/comment
        [HttpPost]
        public async Task<ActionResult<Comment>> AddComment(int taskId, [FromBody] CommentDto dto)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                return NotFound("Task not found.");

            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound("User not found.");

            var comment = new Comment
            {
                TaskId = taskId,
                UserId = dto.UserId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var responseDto = new CommentResponseDto
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                UserName = user.Name,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };

            return Ok(new { message = "Comment created Successfully", comment = responseDto });
        }


        // PUT: api/tasks/{taskId}/comment/{commentId}
        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(int taskId, int commentId, [FromBody] CommentDto dto)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.TaskId == taskId);

            if (comment == null)
                return NotFound("Comment not found.");

            comment.Content = dto.Content;
            comment.CreatedAt = DateTime.UtcNow; // Optional: update timestamp

            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment Updated Successfully", comment });
        }


        // DELETE: api/tasks/{taskId}/comment/{commentId}
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int taskId, int commentId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.TaskId == taskId);

            if (comment == null)
                return NotFound("Comment not found.");

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok("Comment Deleted Successfully");
        }

        // GET: api/tasks/{taskId}/comment/{commentId}


    }
}
