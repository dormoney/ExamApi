using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExamApi.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public int SenderId { get; set; }
        
        [Required]
        public int LessonId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsEdited { get; set; } = false;
        
        public DateTime? EditedAt { get; set; }
        
        [ForeignKey("SenderId")]
        public virtual User? Sender { get; set; }
        
        [ForeignKey("LessonId")]
        public virtual Lesson? Lesson { get; set; }
    }
} 