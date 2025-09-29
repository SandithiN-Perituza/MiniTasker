using mt_backend.Data;
using mt_backend.Models;
using mt_backend.Services.Interfaces;

public class ErrorLogger : IErrorLogger
{
    private readonly MiniTaskerDbContext _context;

    public ErrorLogger(MiniTaskerDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string message, string stackTrace, string source)
    {
        var log = new ErrorLog
        {
            Message = message,
            StackTrace = stackTrace,
            Source = source
        };

        _context.ErrorLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
