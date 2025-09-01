//using System.ComponentModel.DataAnnotations.Schema;

//namespace mt_backend.Models
//{
//    public class Subtask
//    {
//        public int Id { get; set; }
//        public string Title { get; set; }
//        public bool IsCompleted { get; set; }

//        [ForeignKey("Task")]
//        public int TaskId { get; set; }
//        public TaskItem Task { get; set; } // Correct reference to TaskItem
//    }

//}

using System.ComponentModel.DataAnnotations.Schema;

namespace mt_backend.Models
{
    public class Subtask
    {
        public int Id { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string Title { get; set; }

        public bool IsCompleted { get; set; }

        [ForeignKey("Task")]
        public int TaskId { get; set; }
        public TaskItem Task { get; set; }
    }
}