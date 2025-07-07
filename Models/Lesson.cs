using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExamApi.Models
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public int EducationalProgramId { get; set; }
        
        public int? GroupId { get; set; }
        
        public int OrderNumber { get; set; }
        
        public DateTime? ScheduledAt { get; set; }
        
        public int DurationMinutes { get; set; } = 60;
        
        public string? TeacherComment { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("EducationalProgramId")]
        public virtual EducationalProgram? EducationalProgram { get; set; }
        
        [ForeignKey("GroupId")]
        public virtual Group? Group { get; set; }
        
        public virtual ICollection<Material>? Materials { get; set; }
        public virtual ICollection<Attendance>? Attendances { get; set; }
        public virtual ICollection<Message>? Messages { get; set; }
    }
} 