using mt_backend.DTOs;
using mt_backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mt_backend.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskItemDto>> GetTasksAsync();
        Task<TaskItemDto?> GetTaskByIdAsync(int id);
        Task<IEnumerable<TaskItemDto>> GetTasksForUserAsync(string userId);
        Task<TaskItem> CreateTaskAsync(TaskItem task, string actorName);
        Task<TaskItem?> UpdateTaskAsync(int id, TaskItem updatedTask);
        Task<bool> DeleteTaskAsync(int id);
    }
}
