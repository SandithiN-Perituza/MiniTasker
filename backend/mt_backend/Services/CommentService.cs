using Microsoft.EntityFrameworkCore;
using mt_backend.Data;
using mt_backend.DTOs;
using mt_backend.Models;
using mt_backend.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mt_backend.Services
{
    public class CommentService : ICommentService
    {
        private readonly MiniTaskerDbContext _context;

        public CommentService(MiniTaskerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsAsync(int taskId)
        {
            return await _context.Comments
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
        }

        public async Task<CommentResponseDto?> AddCommentAsync(int taskId, CommentDto dto)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return null;

            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return null;

            var comment = new Comment
            {
                TaskId = taskId,
                UserId = dto.UserId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return new CommentResponseDto
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                UserName = user.Name,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<CommentResponseDto?> UpdateCommentAsync(int taskId, int commentId, CommentDto dto)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId && c.TaskId == taskId);

            if (comment == null) return null;

            comment.Content = dto.Content;
            comment.CreatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new CommentResponseDto
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                UserName = comment.User.Name,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<bool> DeleteCommentAsync(int taskId, int commentId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.TaskId == taskId);

            if (comment == null) return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
