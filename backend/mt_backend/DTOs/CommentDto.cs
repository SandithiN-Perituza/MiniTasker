namespace mt_backend.DTOs
{
    public class CommentDto
    {
        public string Content { get; set; }
        public int UserId { get; set; } // You can get this from the logged-in user context
    }
}
