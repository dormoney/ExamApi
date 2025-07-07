using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExamApi.Interfaces;
using ExamApi.Requests;
using System.Security.Claims;

namespace ExamApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            return await _userService.Register(request);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            return await _userService.Login(request);
        }

        [Authorize]
        [HttpPut("UpdateAccount")]
        public async Task<IActionResult> UpdateAccount([FromBody] UpdateUserRequest request)
        {
            var userId = User.FindFirst("Id")?.Value;
            return await _userService.UpdateAccount(userId, request);
        }

        [Authorize]
        [HttpDelete("DeleteAccount")]
        public async Task<IActionResult> DeleteUser()
        {
            var userId = User.FindFirst("Id")?.Value;
            return await _userService.DeleteAccount(userId);
        }
    }
}