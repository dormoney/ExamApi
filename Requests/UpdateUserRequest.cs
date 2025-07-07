namespace ExamApi.Requests
{
    public class UpdateUserRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Password { get; set; }
    }
}