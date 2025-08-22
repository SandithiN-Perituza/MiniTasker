using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mt_backend.Data;
using mt_backend.DTOs;
using mt_backend.Models;
using System;

[ApiController]
[Route("api/tasks/{taskId}/[controller]")]
public class SubtaskController : ControllerBase
{
    private readonly MiniTaskerDbContext _context;

    public SubtaskController(MiniTaskerDbContext context)
    {
        _context = context;
    }

    // GET: api/tasks/{taskId}/subtask
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubtaskDto>>> GetSubtasks(int taskId)
    {
        var subtasks = await _context.Subtasks
            .Where(s => s.TaskId == taskId)
            .Select(s => new SubtaskDto
            {
                Id = s.Id,
                Title = s.Title,
                IsCompleted = s.IsCompleted
            })
            .ToListAsync();

        return Ok(subtasks);
    }

    // POST: api/tasks/{taskId}/subtask
    [HttpPost]
    public async Task<ActionResult<SubtaskDto>> CreateSubtask(int taskId, CreateSubtaskDto dto)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null)
        {
            return NotFound("Task not found.");
        }

        var subtask = new Subtask
        {
            Title = dto.Title,
            IsCompleted = false,
            TaskId = taskId
        };

        _context.Subtasks.Add(subtask);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSubtasks), new { taskId = taskId }, new SubtaskDto
        {
            Id = subtask.Id,
            Title = subtask.Title,
            IsCompleted = subtask.IsCompleted
        });
    }

    // PATCH: api/tasks/{taskId}/subtask/{id}
    [HttpPatch("{id}")]
    public async Task<IActionResult> MarkSubtaskCompleted(int taskId, int id)
    {
        var subtask = await _context.Subtasks
            .FirstOrDefaultAsync(s => s.Id == id && s.TaskId == taskId);

        if (subtask == null)
        {
            return NotFound("Subtask not found.");
        }

        subtask.IsCompleted = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "SubTask Updated Successfully", subtask });
    }

    // PUT: api/tasks/{taskId}/subtask/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubtask(int taskId, int id, CreateSubtaskDto dto)
    {
        var subtask = await _context.Subtasks
            .FirstOrDefaultAsync(s => s.Id == id && s.TaskId == taskId);

        if (subtask == null)
        {
            return NotFound("Subtask not found.");
        }

        subtask.Title = dto.Title;
        await _context.SaveChangesAsync();

        return Ok(new { message = "SubTask Updated Successfully", subtask });
    }

    // DELETE: api/tasks/{taskId}/subtask/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubtask(int taskId, int id)
    {
        var subtask = await _context.Subtasks
            .FirstOrDefaultAsync(s => s.Id == id && s.TaskId == taskId);

        if (subtask == null)
        {
            return NotFound("Subtask not found.");
        }

        _context.Subtasks.Remove(subtask);
        await _context.SaveChangesAsync();

        return Ok(new { mesage =  "SubTask deleted Successfully" }); ;
    }
}

