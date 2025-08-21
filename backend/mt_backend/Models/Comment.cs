using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace mt_backend.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [ForeignKey("Task")]
        public int TaskId { get; set; }
        public TaskItem Task { get; set; }        
        
        [ForeignKey("User")]
        public int UserId { get; set; }
         public User User { get; set; }       
        
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
       
    }
}
