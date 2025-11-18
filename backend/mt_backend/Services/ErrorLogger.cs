using BackendExamples;
using mt_backend.Data;
using mt_backend.Models;
using mt_backend.Services.Interfaces;

namespace mt_backend.Services
{
    public class ErrorLogger : IErrorLogger
    {
        private readonly MiniTaskerDbContext _context;

        public ErrorLogger(MiniTaskerDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string message, string stackTrace, string source)
        {
            try
            {
                // Log to console first for immediate visibility
                Console.WriteLine($"📝 ERROR LOG ATTEMPT: [{source}] {message}");
                
                var log = new ErrorLog
                {
                    Message = message,
                    StackTrace = stackTrace,
                    Source = source,
                    Timestamp = DateTime.UtcNow
                };

                _context.ErrorLogs.Add(log);
                await _context.SaveChangesAsync();

                // Success log to console
                Console.WriteLine($"✅ ERROR LOG SAVED: [{source}] {message}");
                if (!string.IsNullOrEmpty(stackTrace) && stackTrace != "No stack trace")
                {
                    Console.WriteLine($"   Stack: {stackTrace}");
                }
            }
            catch (Exception ex)
            {
                // If logging fails, at least log to console
                Console.WriteLine($"❌ LOGGING FAILED: {ex.Message}");
                Console.WriteLine($"   Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"   Exception Stack: {ex.StackTrace}");
                Console.WriteLine($"   Original message: {message}");
                Console.WriteLine($"   Original source: {source}");

                // Try to get more details about the database connection
                try
                {
                    var canConnect = await _context.Database.CanConnectAsync();
                    Console.WriteLine($"   Database CanConnect: {canConnect}");
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"   Database Connection Test Failed: {dbEx.Message}");
                }
            }
        }


        public async Task LogAsync(ErrorLogDto dto)
        {
            try
            {
                var log = new ErrorLog
                {
                    Message = dto.Message ?? "(no message)",
                    StackTrace = dto.StackTrace ?? "No stack trace",
                    Source = dto.Source ?? "frontend",
                    Timestamp = dto.Timestamp != default ? dto.Timestamp : DateTime.UtcNow
                };

                _context.ErrorLogs.Add(log);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ ERROR LOG SAVED FROM DTO: [{log.Source}] {log.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LOGGING FAILED (DTO): {ex.Message}");
            }
        }

    }
}
