using mt_backend.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace mt_backend.DTOs
{//for outgoing data (from server to client)
 // Used when returning subtask data to the client.
 // Includes fields like Id, IsCompleted, etc.
    public class SubtaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }

}
