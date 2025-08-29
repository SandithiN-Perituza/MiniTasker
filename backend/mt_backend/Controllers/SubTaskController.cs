using Microsoft.AspNetCore.Mvc;
using mt_backend.DTOs;
using mt_backend.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/tasks/{taskId}/[controller]")]
public class SubtaskController : ControllerBase
{
    private readonly ISubtaskService _subtaskService;

    public SubtaskController(ISubtaskService subtaskService)
    {
        _subtaskService = subtaskService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubtaskDto>>> GetSubtasks(int taskId)
    {
        var subtasks = await _subtaskService.GetSubtasksAsync(taskId);
        return Ok(subtasks);
    }

    [HttpPost]
    public async Task<ActionResult<SubtaskDto>> CreateSubtask(int taskId, CreateSubtaskDto dto)
    {
        var subtask = await _subtaskService.CreateSubtaskAsync(taskId, dto);
        if (subtask == null) return NotFound("Task not found.");

        return CreatedAtAction(nameof(GetSubtasks), new { taskId }, subtask);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> MarkSubtaskCompleted(int taskId, int id)
    {
        var subtask = await _subtaskService.MarkSubtaskCompletedAsync(taskId, id);
        if (subtask == null) return NotFound("Subtask not found.");

        return Ok(new { message = "SubTask Updated Successfully", subtask });
    }

    [HttpPatch("{id}/mark-incomplete")]
    public async Task<IActionResult> MarkSubtaskIncomplete(int taskId, int id)
    {
        var subtask = await _subtaskService.MarkSubtaskIncompleteAsync(taskId, id);
        if (subtask == null) return NotFound("Subtask not found.");

        return Ok(new { message = "SubTask marked as incomplete successfully", subtask });
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubtask(int taskId, int id, CreateSubtaskDto dto)
    {
        var subtask = await _subtaskService.UpdateSubtaskAsync(taskId, id, dto);
        if (subtask == null) return NotFound("Subtask not found.");

        return Ok(new { message = "SubTask Updated Successfully", subtask });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubtask(int taskId, int id)
    {
        var success = await _subtaskService.DeleteSubtaskAsync(taskId, id);
        if (!success) return NotFound("Subtask not found.");

        return Ok(new { message = "SubTask deleted Successfully" });
    }
}
