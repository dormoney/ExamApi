using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExamApi.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public int LessonId { get; set; }
        
        public string? Content { get; set; }
        
        public string? Link { get; set; }
        
        public string Type { get; set; } = "Text";
        
        public int OrderNumber { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("LessonId")]
        public virtual Lesson? Lesson { get; set; }
    }
} 