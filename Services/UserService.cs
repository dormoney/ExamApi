using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExamApi.Data;
using ExamApi.Interfaces;
using ExamApi.Models;
using ExamApi.Requests;

namespace ExamApi.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public UserService(ApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return new BadRequestObjectResult(new { Message = "User with this email already exists" });
            }

            var user = new User
            {
                Email = request.Email,
                Password = request.Password,
                Name = request.Name,
                Description = request.Description,
                Role = request.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role);

            return new OkObjectResult(new
            {
                Token = token,
                User = new
                {
                    user.Id,
                    user.Email,
                    user.Name,
                    user.Role
                }
            });
        }

        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || user.Password != request.Password)
            {
                return new BadRequestObjectResult(new { Message = "Invalid email or password" });
            }

            var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role);

            return new OkObjectResult(new
            {
                Token = token,
                User = new
                {
                    user.Id,
                    user.Email,
                    user.Name,
                    user.Role
                }
            });
        }

        public async Task<IActionResult> UpdateAccount(string userId, UpdateUserRequest request)
        {
            if (!int.TryParse(userId, out int id))
            {
                return new BadRequestObjectResult(new { Message = "Invalid user ID" });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return new NotFoundObjectResult(new { Message = "User not found" });
            }

            if (!string.IsNullOrEmpty(request.Name))
                user.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description))
                user.Description = request.Description;
            if (!string.IsNullOrEmpty(request.Password))
                user.Password = request.Password;

            await _context.SaveChangesAsync();

            return new OkObjectResult(new { Message = "Account updated successfully" });
        }

        public async Task<IActionResult> DeleteAccount(string userId)
        {
            if (!int.TryParse(userId, out int id))
            {
                return new BadRequestObjectResult(new { Message = "Invalid user ID" });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return new NotFoundObjectResult(new { Message = "User not found" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { Message = "Account deleted successfully" });
        }

        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return new NotFoundObjectResult(new { Message = "User not found" });
            }

            return new OkObjectResult(new
            {
                user.Id,
                user.Email,
                user.Name,
                user.Role,
                user.CreatedAt
            });
        }

        public async Task<IActionResult> GetAllUsers(int onPage, int page)
        {
            var users = await _context.Users
                .Skip((page - 1) * onPage)
                .Take(onPage)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.Name,
                    u.Role,
                    u.CreatedAt
                })
                .ToListAsync();

            return new OkObjectResult(users);
        }

        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return new NotFoundObjectResult(new { Message = "User not found" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { Message = "User deleted successfully" });
        }

        public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest user)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return new NotFoundObjectResult(new { Message = "User not found" });
            }

            if (!string.IsNullOrEmpty(user.Name))
                existingUser.Name = user.Name;
            if (!string.IsNullOrEmpty(user.Description))
                existingUser.Description = user.Description;
            if (!string.IsNullOrEmpty(user.Password))
                existingUser.Password = user.Password;

            await _context.SaveChangesAsync();

            return new OkObjectResult(new { Message = "User updated successfully" });
        }


    }
}