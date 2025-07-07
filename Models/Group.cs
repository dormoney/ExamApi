using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExamApi.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public int EducationalProgramId { get; set; }
        
        [Required]
        public int TeacherId { get; set; }
        
        public int MaxStudents { get; set; } = 10;
        
        public bool IsOpen { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("EducationalProgramId")]
        public virtual EducationalProgram? EducationalProgram { get; set; }
        
        [ForeignKey("TeacherId")]
        public virtual User? Teacher { get; set; }
        
        public virtual ICollection<User>? Students { get; set; }
        public virtual ICollection<Lesson>? Lessons { get; set; }
    }
} 