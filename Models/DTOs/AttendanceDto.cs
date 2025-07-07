namespace ExamApi.Models.DTOs
{
    public class AttendanceDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int LessonId { get; set; }
        public bool IsPresent { get; set; }
        public DateTime? MarkedAt { get; set; }
        public int? MarkedByTeacherId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string LessonTitle { get; set; } = string.Empty;
        public string? MarkedByTeacherName { get; set; }
    }

    public class CreateAttendanceDto
    {
        public int StudentId { get; set; }
        public int LessonId { get; set; }
        public bool IsPresent { get; set; }
    }

    public class UpdateAttendanceDto
    {
        public bool IsPresent { get; set; }
    }

    public class CreateBulkAttendanceDto
    {
        public int LessonId { get; set; }
        public List<StudentAttendanceDto> Students { get; set; } = new();
    }

    public class StudentAttendanceDto
    {
        public int StudentId { get; set; }
        public bool IsPresent { get; set; }
    }
} 