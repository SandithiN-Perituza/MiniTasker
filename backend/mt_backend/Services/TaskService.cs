using Microsoft.EntityFrameworkCore;
using mt_backend.Data;
using mt_backend.DTOs;
using mt_backend.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mt_backend.Services
{
    public class TaskService : ITaskService
    {
        private readonly MiniTaskerDbContext _context;

        public TaskService(MiniTaskerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskItemDto>> GetTasksAsync()
        {
            return await _context.Tasks
                .Include(t => t.AssignedUser)
                .Select(t => new TaskItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    AssignedTo = t.AssignedTo ?? 0,
                    AssignedUserName = t.AssignedUser.Name,
                    DueDate = t.DueDate
                })
                .ToListAsync();
        }

        public async Task<TaskItemDto> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.AssignedUser)
                .Where(t => t.Id == id)
                .Select(t => new TaskItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    AssignedTo = t.AssignedTo ?? 0,
                    AssignedUserName = t.AssignedUser.Name,
                    DueDate = t.DueDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TaskItem> CreateTaskAsync(TaskItem task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem> UpdateTaskAsync(int id, TaskItem updatedTask)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return null;

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.Status = updatedTask.Status;
            task.AssignedTo = updatedTask.AssignedTo;
            task.DueDate = updatedTask.DueDate;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
