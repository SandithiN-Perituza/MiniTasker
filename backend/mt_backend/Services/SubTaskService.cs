using Microsoft.EntityFrameworkCore;
using mt_backend.Data;
using mt_backend.DTOs;
using mt_backend.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mt_backend.Services
{
    public class SubtaskService : ISubtaskService
    {
        private readonly MiniTaskerDbContext _context;

        public SubtaskService(MiniTaskerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SubtaskDto>> GetSubtasksAsync(int taskId)
        {
            return await _context.Subtasks
                .Where(s => s.TaskId == taskId)
                .Select(s => new SubtaskDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    IsCompleted = s.IsCompleted
                })
                .ToListAsync();
        }

        public async Task<SubtaskDto?> CreateSubtaskAsync(int taskId, CreateSubtaskDto dto)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return null;

            var subtask = new Subtask
            {
                Title = dto.Title,
                IsCompleted = false,
                TaskId = taskId
            };

            _context.Subtasks.Add(subtask);
            await _context.SaveChangesAsync();

            return new SubtaskDto
            {
                Id = subtask.Id,
                Title = subtask.Title,
                IsCompleted = subtask.IsCompleted
            };
        }

        public async Task<SubtaskDto?> MarkSubtaskCompletedAsync(int taskId, int subtaskId)
        {
            var subtask = await _context.Subtasks
                .FirstOrDefaultAsync(s => s.Id == subtaskId && s.TaskId == taskId);

            if (subtask == null) return null;

            subtask.IsCompleted = true;
            await _context.SaveChangesAsync();

            return new SubtaskDto
            {
                Id = subtask.Id,
                Title = subtask.Title,
                IsCompleted = subtask.IsCompleted
            };
        }

        public async Task<SubtaskDto> MarkSubtaskIncompleteAsync(int taskId, int subtaskId)
        {
            var subtask = await _context.Subtasks
                .FirstOrDefaultAsync(s => s.TaskId == taskId && s.Id == subtaskId);

            if (subtask == null) return null;

            subtask.IsCompleted = false;
            await _context.SaveChangesAsync();

            return new SubtaskDto
            {
                Id = subtask.Id,
                Title = subtask.Title,
                IsCompleted = subtask.IsCompleted,
            };
        }



        public async Task<SubtaskDto?> UpdateSubtaskAsync(int taskId, int subtaskId, CreateSubtaskDto dto)
        {
            var subtask = await _context.Subtasks
                .FirstOrDefaultAsync(s => s.Id == subtaskId && s.TaskId == taskId);

            if (subtask == null) return null;

            subtask.Title = dto.Title;
            await _context.SaveChangesAsync();

            return new SubtaskDto
            {
                Id = subtask.Id,
                Title = subtask.Title,
                IsCompleted = subtask.IsCompleted
            };
        }

        public async Task<bool> DeleteSubtaskAsync(int taskId, int subtaskId)
        {
            var subtask = await _context.Subtasks
                .FirstOrDefaultAsync(s => s.Id == subtaskId && s.TaskId == taskId);

            if (subtask == null) return false;

            _context.Subtasks.Remove(subtask);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
