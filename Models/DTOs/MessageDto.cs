namespace ExamApi.Models.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int SenderId { get; set; }
        public int LessonId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string LessonTitle { get; set; } = string.Empty;
    }

    public class CreateMessageDto
    {
        public string Content { get; set; } = string.Empty;
        public int LessonId { get; set; }
    }

    public class UpdateMessageDto
    {
        public string Content { get; set; } = string.Empty;
    }
} 