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
    public class TaskService : ITaskService
    {
        private readonly MiniTaskerDbContext _context;

        public TaskService(MiniTaskerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskItemDto>> GetTasksAsync()
        {
            try
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
                        AssignedUserName = t.AssignedUser != null ? t.AssignedUser.Name : null,
                        AssignedUserAzureAdId = t.AssignedUser != null ? t.AssignedUser.AzureAdId : null,
                        DueDate = t.DueDate
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTasksAsync: {ex}");
                throw;
            }
        }

        public async Task<IEnumerable<TaskItemDto>> GetTasksForUserAsync(string microsoftUserId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.AzureAdId == microsoftUserId);

            if (user == null)
            {
                return new List<TaskItemDto>();
            }

            var tasks = await _context.Tasks
                .Where(t => t.AssignedTo == user.Id)
                .Include(t => t.AssignedUser)
                .ToListAsync();

            var taskDtos = tasks.Select(t => new TaskItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status.ToString(),
                AssignedTo = t.AssignedTo ?? 0,
                AssignedUserName = t.AssignedUser?.Name,
                AssignedUserAzureAdId = t.AssignedUser?.AzureAdId,
                DueDate = t.DueDate
            });

            return taskDtos;
        }

        public async Task<TaskItemDto?> GetTaskByIdAsync(int id)
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
                    AssignedUserName = t.AssignedUser != null ? t.AssignedUser.Name : null,
                    AssignedUserAzureAdId = t.AssignedUser != null ? t.AssignedUser.AzureAdId : null,
                    DueDate = t.DueDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TaskItem> CreateTaskAsync(TaskItem task, string actorName)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem?> UpdateTaskAsync(int id, TaskItem updatedTask)
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