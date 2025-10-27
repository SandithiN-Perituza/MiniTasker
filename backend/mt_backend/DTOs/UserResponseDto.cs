namespace mt_backend.DTOs
{
    public class UserResponseDto
    {
        //UserResponse is a DTO (Data Transfer Object) used to send user data back to the client
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";

    }
}

