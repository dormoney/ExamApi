namespace ExamApi.Models.DTOs
{
    public class EducationalProgramDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateEducationalProgramDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateEducationalProgramDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class EducationalProgramDetailDto : EducationalProgramDto
    {
        public List<ProgramGroupDto> Groups { get; set; } = new();
        public List<ProgramLessonDto> Lessons { get; set; } = new();
    }

    public class ProgramGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int StudentsCount { get; set; }
        public bool IsOpen { get; set; }
    }

    public class ProgramLessonDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int OrderNumber { get; set; }
        public DateTime? ScheduledAt { get; set; }
    }
} 