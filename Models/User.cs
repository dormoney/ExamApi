using System.ComponentModel.DataAnnotations;

namespace ExamApi.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public string Role { get; set; } = "Student";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual ICollection<Group>? GroupsAsStudent { get; set; }
        public virtual ICollection<Group>? GroupsAsTeacher { get; set; }
        public virtual ICollection<Attendance>? Attendances { get; set; }
        public virtual ICollection<Message>? Messages { get; set; }
    }
} 