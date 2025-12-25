using System.ComponentModel.DataAnnotations.Schema;

namespace mt_backend.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? Title { get; set; } = "";

        [Column(TypeName = "text")]
        public string Description { get; set; } = "";

        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime")]
        public DateTime DueDate { get; set; }

        [ForeignKey("AssignedUser")]
        public int? AssignedTo { get; set; }
        public User? AssignedUser { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Subtask> Subtasks { get; set; } = new List<Subtask>();
    }
}

//using System.ComponentModel.DataAnnotations.Schema;

//namespace mt_backend.Models
//{
//    public class TaskItem
//    {
//        public int Id { get; set; }
//        public string? Title { get; set; } = "";
//        public string Description { get; set; } = "";
//        public TaskStatus Status { get; set; } = TaskStatus.Pending; 

//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

//        public DateTime DueDate { get; set; }

//        [ForeignKey("AssignedUser")]
//        public int? AssignedTo { get; set; }
//        public User? AssignedUser { get; set; }


//        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
//        public ICollection<Subtask> Subtasks { get; set; } = new List<Subtask>();

//    }
//}