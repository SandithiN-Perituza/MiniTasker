using System;

namespace BackendExamples
{
    public class ErrorLogDto
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Source { get; set; }
        public DateTime Timestamp { get; set; }
    }
}