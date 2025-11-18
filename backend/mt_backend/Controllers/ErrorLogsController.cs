using BackendExamples;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mt_backend.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace BackendExamples.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ErrorLogsController : ControllerBase
    {
        private readonly IErrorLogger _errorLogger;

        public ErrorLogsController(IErrorLogger errorLogger)
        {
            _errorLogger = errorLogger;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] ErrorLogDto dto)
        {
            if (dto == null) return BadRequest();

            try
            {
                await _errorLogger.LogAsync(dto);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to save error log: {ex.Message}");
                return StatusCode(500, "Failed to log error");
            }
        }
    }
}