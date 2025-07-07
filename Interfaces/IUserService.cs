using Microsoft.AspNetCore.Mvc;
using ExamApi.Requests;

namespace ExamApi.Interfaces
{
    public interface IUserService
    {
        Task<IActionResult> Register(RegisterRequest request);
        Task<IActionResult> Login(LoginRequest request);
        Task<IActionResult> UpdateAccount(string userId, UpdateUserRequest request);
        Task<IActionResult> DeleteAccount(string userId);
        Task<IActionResult> GetUserByEmail(string email);
        Task<IActionResult> GetAllUsers(int onPage, int page);
        Task<IActionResult> DeleteUser(int id);
        Task<IActionResult> UpdateUser(int id, UpdateUserRequest user);
    }
}