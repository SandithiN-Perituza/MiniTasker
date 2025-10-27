using System.Text.Json.Serialization;

namespace mt_backend.DTOs
{
    public class CreateTaskWithNotificationRequestDto
    {
        public TaskDataDto? Task { get; set; }
        public string? Message { get; set; }
        public string? UserId { get; set; }  // Creator's Azure AD ID
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public string? AssignedUserAzureAdId { get; set; }  // Assigned user's Azure AD ID
        public string? AuthToken { get; set; }  // Backend auth token
        public string? GraphToken { get; set; }  // Access token for Graph API
        public string? Timestamp { get; set; }
    }

    public class TaskDataDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }

        [JsonPropertyName("assignedTo")]
        public int AssignedTo { get; set; }
        public DateTime DueDate { get; set; }
    }
}