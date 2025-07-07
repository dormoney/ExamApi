using Microsoft.AspNetCore.Mvc;

namespace ExamApi.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string email, string role);
        Task<IActionResult> ValidateToken(string token);
    }
}