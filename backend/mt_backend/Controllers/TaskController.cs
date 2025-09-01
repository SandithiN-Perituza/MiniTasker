using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using mt_backend.DTOs;
using mt_backend.Models;
using mt_backend.Services;
using mt_backend.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mt_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly INotificationService _notifier;
        public TasksController(ITaskService taskService, INotificationService notifier)
        {
            _taskService = taskService;
            _notifier = notifier;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasks()
        {
            var tasks = await _taskService.GetTasksAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItemDto>> GetTaskById(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
        {
            var createdTask = await _taskService.CreateTaskAsync(task);


            await _notifier.SendMessageAsync($"New Task Created: **{createdTask.Title}** assigned to **{createdTask.AssignedTo}**");

            return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem updatedTask)
        {
            if (id != updatedTask.Id) return BadRequest();

            var task = await _taskService.UpdateTaskAsync(id, updatedTask);

            if (task == null) return NotFound(new { message = $"Task with ID {id} not found." });

            await _notifier.SendMessageAsync($"Task Updated: **{updatedTask.Title}** assigned to **{updatedTask.AssignedTo}**");

            return Ok(task);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var deletedTask = await _taskService.GetTaskByIdAsync(id);
            var success = await _taskService.DeleteTaskAsync(id);
            if (!success) return NotFound();

            await _notifier.SendMessageAsync($"Task with Id: {deletedTask.Id} and title {deletedTask.Title} is deleted");

            return Ok(new { message = $"Task with ID {id} is deleted successfully" });
        }
    }
}
