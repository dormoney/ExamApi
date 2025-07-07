using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExamApi.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int StudentId { get; set; }
        
        [Required]
        public int LessonId { get; set; }
        
        public bool IsPresent { get; set; } = false;
        
        public DateTime? MarkedAt { get; set; }
        
        public int? MarkedByTeacherId { get; set; }
        
        [ForeignKey("StudentId")]
        public virtual User? Student { get; set; }
        
        [ForeignKey("LessonId")]
        public virtual Lesson? Lesson { get; set; }
        
        [ForeignKey("MarkedByTeacherId")]
        public virtual User? MarkedByTeacher { get; set; }
    }
} 