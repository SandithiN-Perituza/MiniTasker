namespace mt_backend.Models
{
    public class ErrorLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Source { get; set; }
    }

}
