using Humanizer;
using mt_backend.Models;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace mt_backend.DTOs
{
//— for incoming data(from client to server)
//Used when creating a new subtask.
//Only includes fields the client needs to provide.
//Keeps the API clean and avoids exposing internal or auto-generated fields like Id
    public class CreateSubtaskDto
    {
        public string Title { get; set; } = string.Empty;
    }
}
