namespace mt_backend.DTOs
{
    public class CreateTaskRequestDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int AssignedTo { get; set; }
        public DateTime DueDate { get; set; }
        public string? ActorName { get; set; }
    }
}
