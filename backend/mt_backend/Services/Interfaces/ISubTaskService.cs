using mt_backend.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mt_backend.Services
{
    public interface ISubtaskService
    {
        Task<IEnumerable<SubtaskDto>> GetSubtasksAsync(int taskId);
        Task<SubtaskDto?> CreateSubtaskAsync(int taskId, CreateSubtaskDto dto);
        Task<SubtaskDto?> MarkSubtaskCompletedAsync(int taskId, int subtaskId);
        Task<SubtaskDto?> UpdateSubtaskAsync(int taskId, int subtaskId, CreateSubtaskDto dto);
        Task<bool> DeleteSubtaskAsync(int taskId, int subtaskId);
        Task<SubtaskDto> MarkSubtaskIncompleteAsync(int taskId, int subtaskId);
    }
}
