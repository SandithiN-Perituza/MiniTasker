using System.ComponentModel.DataAnnotations.Schema;

namespace mt_backend.Models
{
    public class User
    {
        public int Id { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string? AzureAdId { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string Name { get; set; } = "";

        [Column(TypeName = "varchar(255)")]
        public string Email { get; set; } = "";

        [Column(TypeName = "varchar(255)")]
        public string Password { get; set; } = "";

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}

//using System.Collections;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace mt_backend.Models
//{
//    public class User
//    {
//        public int Id { get; set; }
//        public string Name { get; set; } = "";
//        public string Email { get; set; } = "";

//        public string Password { get; set; } = "";
//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

//        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
//    }
//}