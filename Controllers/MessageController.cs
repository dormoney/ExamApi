using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExamApi.Data;
using ExamApi.Models;
using ExamApi.Models.DTOs;
using System.Security.Claims;

namespace ExamApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessageController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages()
        {
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Lesson)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    SenderId = m.SenderId,
                    LessonId = m.LessonId,
                    CreatedAt = m.CreatedAt,
                    IsEdited = m.IsEdited,
                    EditedAt = m.EditedAt,
                    SenderName = m.Sender.Name,
                    LessonTitle = m.Lesson.Title
                })
                .ToListAsync();

            return messages;
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<MessageDto>> GetMessage(int id)
        {
            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Lesson)
                .Where(m => m.Id == id)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    SenderId = m.SenderId,
                    LessonId = m.LessonId,
                    CreatedAt = m.CreatedAt,
                    IsEdited = m.IsEdited,
                    EditedAt = m.EditedAt,
                    SenderName = m.Sender.Name,
                    LessonTitle = m.Lesson.Title
                })
                .FirstOrDefaultAsync();

            if (message == null)
            {
                return NotFound();
            }

            return message;
        }

        [Authorize]
        [HttpGet("Lesson/{lessonId}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesByLesson(int lessonId)
        {
            var userId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var lesson = await _context.Lessons
                .Include(l => l.Group)
                    .ThenInclude(g => g.Students)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
            {
                return NotFound("Lesson not found");
            }

            bool hasAccess = false;

            if (userRole == "Admin")
            {
                hasAccess = true;
            }
            else if (userRole == "Teacher")
            {
                hasAccess = lesson.Group?.TeacherId == userId;
            }
            else if (userRole == "Student")
            {
                hasAccess = lesson.Group?.Students.Any(s => s.Id == userId) == true;
            }

            if (!hasAccess)
            {
                return Unauthorized("You don't have access to this lesson");
            }

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.LessonId == lessonId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    SenderId = m.SenderId,
                    LessonId = m.LessonId,
                    CreatedAt = m.CreatedAt,
                    IsEdited = m.IsEdited,
                    EditedAt = m.EditedAt,
                    SenderName = m.Sender.Name,
                    LessonTitle = m.Lesson.Title
                })
                .ToListAsync();

            return messages;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage([FromBody] CreateMessageDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var lesson = await _context.Lessons
                .Include(l => l.Group)
                    .ThenInclude(g => g.Students)
                .FirstOrDefaultAsync(l => l.Id == createDto.LessonId);
            if (lesson == null)
            {
                return BadRequest("Lesson not found");
            }

            bool hasAccess = false;

            if (userRole == "Admin")
            {
                hasAccess = true;
            }
            else if (userRole == "Teacher")
            {
                hasAccess = lesson.Group?.TeacherId == userId;
            }
            else if (userRole == "Student")
            {
                hasAccess = lesson.Group?.Students.Any(s => s.Id == userId) == true;
            }

            if (!hasAccess)
            {
                return Unauthorized("You don't have access to this lesson");
            }

            var message = new Message
            {
                Content = createDto.Content,
                LessonId = createDto.LessonId,
                SenderId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _context.Entry(message).Reference(m => m.Sender).LoadAsync();

            var messageDto = new MessageDto
            {
                Id = message.Id,
                Content = message.Content,
                SenderId = message.SenderId,
                LessonId = message.LessonId,
                CreatedAt = message.CreatedAt,
                IsEdited = message.IsEdited,
                EditedAt = message.EditedAt,
                SenderName = message.Sender.Name,
                LessonTitle = lesson.Title
            };

            return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, messageDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMessage(int id, [FromBody] UpdateMessageDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            if (message.SenderId != userId)
            {
                return Unauthorized("You can only edit your own messages");
            }

            message.Content = updateDto.Content;
            message.IsEdited = true;
            message.EditedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (message.SenderId != userId && userRole != "Admin")
            {
                return Unauthorized("You can only delete your own messages");
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MessageExists(int id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }
    }
} 