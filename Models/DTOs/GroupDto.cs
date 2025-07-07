namespace ExamApi.Models.DTOs
{
    public class GroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int EducationalProgramId { get; set; }
        public int TeacherId { get; set; }
        public int MaxStudents { get; set; }
        public bool IsOpen { get; set; }
        public DateTime CreatedAt { get; set; }
        public string EducationalProgramName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int StudentsCount { get; set; }
    }

    public class CreateGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int EducationalProgramId { get; set; }
        public int TeacherId { get; set; }
        public int MaxStudents { get; set; } = 10;
        public bool IsOpen { get; set; } = true;
    }

    public class UpdateGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxStudents { get; set; }
        public bool IsOpen { get; set; }
    }

    public class GroupDetailDto : GroupDto
    {
        public List<GroupStudentDto> Students { get; set; } = new();
        public List<GroupLessonDto> Lessons { get; set; } = new();
    }

    public class GroupStudentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class GroupLessonDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime? ScheduledAt { get; set; }
        public int OrderNumber { get; set; }
    }
} 