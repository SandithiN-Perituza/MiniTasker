namespace mt_backend.DTOs
{
    public class NotificationResultDto
    {
        public bool Success { get; set; }
        public string Method { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string? RecipientId { get; set; }
        public string? SenderName { get; set; }
        public string? TokenType { get; set; }
        public string? ErrorDetails { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}