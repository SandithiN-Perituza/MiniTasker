namespace mt_backend.DTOs
{
    public class TaskItemDto
    {

        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public int AssignedTo { get; set; }
        public string? AssignedUserName { get; set; }
        public DateTime DueDate { get; set; }

    }
}
