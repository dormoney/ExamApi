namespace ExamApi.Models.DTOs
{
    public class LessonDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int EducationalProgramId { get; set; }
        public int? GroupId { get; set; }
        public int OrderNumber { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public int DurationMinutes { get; set; }
        public string? TeacherComment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string EducationalProgramName { get; set; } = string.Empty;
        public string? GroupName { get; set; }
    }

    public class CreateLessonDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int EducationalProgramId { get; set; }
        public int? GroupId { get; set; }
        public int OrderNumber { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public int DurationMinutes { get; set; } = 60;
    }

    public class UpdateLessonDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderNumber { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public int DurationMinutes { get; set; }
        public string? TeacherComment { get; set; }
    }

    public class LessonDetailDto : LessonDto
    {
        public List<LessonMaterialDto> Materials { get; set; } = new();
        public List<LessonAttendanceDto> Attendances { get; set; } = new();
        public List<LessonMessageDto> Messages { get; set; } = new();
    }

    public class LessonMaterialDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int OrderNumber { get; set; }
    }

    public class LessonAttendanceDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
    }

    public class LessonMessageDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsEdited { get; set; }
    }
} 