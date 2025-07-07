namespace ExamApi.Models.DTOs
{
    public class MaterialDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int LessonId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? Link { get; set; }
        public string Type { get; set; } = string.Empty;
        public int OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
    }

    public class CreateMaterialDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int LessonId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? Link { get; set; }
        public string Type { get; set; } = string.Empty;
        public int OrderNumber { get; set; }
    }

    public class UpdateMaterialDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? Link { get; set; }
        public string Type { get; set; } = string.Empty;
        public int OrderNumber { get; set; }
    }
} 