using mt_backend.DTOs;
using mt_backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mt_backend.Services.Interfaces
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentResponseDto>> GetCommentsAsync(int taskId);
        Task<CommentResponseDto?> AddCommentAsync(int taskId, CommentDto dto);
        Task<CommentResponseDto?> UpdateCommentAsync(int taskId, int commentId, CommentDto dto);
        Task<bool> DeleteCommentAsync(int taskId, int commentId);
    }
}

