namespace MyFullStackApp.Backend.Models
{    
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class TodoItem
    {
        public static readonly List<string> validSortFields = new() {"id", "title"};

        [Key]
        public int Id { get; set; }

        [Required]
        public required string Title { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 關聯到 User
        [Required]
        public required string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } // EF Core 自動處理
    }
}