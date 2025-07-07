using System.ComponentModel.DataAnnotations;

namespace ExamApi.Models
{
    public class EducationalProgram
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Навигационные свойства
        public virtual ICollection<Group>? Groups { get; set; }
        public virtual ICollection<Lesson>? Lessons { get; set; }
    }
} 