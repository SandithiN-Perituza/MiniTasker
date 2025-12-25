using Humanizer;
using mt_backend.DTOs;
using mt_backend.Models;
using System.Security.Cryptography.Xml;
//CreateUserRequest is a Data Transfer Object(DTO) used to receive user input when creating a new user via your API.
namespace mt_backend.DTOs
{
    public class CreateUserRequestDto
    {
            public string Name { get; set; } = "";
            public string Email { get; set; } = "";
            public string Password { get; set; } = "";
    }

}

