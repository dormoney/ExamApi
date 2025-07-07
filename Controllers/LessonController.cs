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
    public class LessonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LessonController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LessonDto>>> GetLessons()
        {
            var lessons = await _context.Lessons
                .Include(l => l.EducationalProgram)
                .Include(l => l.Group)
                .Include(l => l.Materials)
                .Select(l => new LessonDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    EducationalProgramId = l.EducationalProgramId,
                    GroupId = l.GroupId,
                    OrderNumber = l.OrderNumber,
                    ScheduledAt = l.ScheduledAt,
                    DurationMinutes = l.DurationMinutes,
                    TeacherComment = l.TeacherComment,
                    CreatedAt = l.CreatedAt,
                    EducationalProgramName = l.EducationalProgram.Name,
                    GroupName = l.Group != null ? l.Group.Name : null
                })
                .ToListAsync();

            return lessons;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LessonDetailDto>> GetLesson(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.EducationalProgram)
                .Include(l => l.Group)
                .Include(l => l.Materials)
                .Include(l => l.Attendances)
                    .ThenInclude(a => a.Student)
                .Include(l => l.Messages)
                    .ThenInclude(m => m.Sender)
                .Where(l => l.Id == id)
                .Select(l => new LessonDetailDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    EducationalProgramId = l.EducationalProgramId,
                    GroupId = l.GroupId,
                    OrderNumber = l.OrderNumber,
                    ScheduledAt = l.ScheduledAt,
                    DurationMinutes = l.DurationMinutes,
                    TeacherComment = l.TeacherComment,
                    CreatedAt = l.CreatedAt,
                    EducationalProgramName = l.EducationalProgram.Name,
                    GroupName = l.Group != null ? l.Group.Name : null,
                    Materials = l.Materials.Select(m => new LessonMaterialDto
                    {
                        Id = m.Id,
                        Title = m.Title,
                        Type = m.Type,
                        OrderNumber = m.OrderNumber
                    }).ToList(),
                    Attendances = l.Attendances.Select(a => new LessonAttendanceDto
                    {
                        Id = a.Id,
                        StudentId = a.StudentId,
                        StudentName = a.Student.Name,
                        IsPresent = a.IsPresent
                    }).ToList(),
                    Messages = l.Messages.Select(m => new LessonMessageDto
                    {
                        Id = m.Id,
                        Content = m.Content,
                        SenderName = m.Sender.Name,
                        CreatedAt = m.CreatedAt,
                        IsEdited = m.IsEdited
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (lesson == null)
            {
                return NotFound();
            }

            return lesson;
        }

        [HttpGet("Program/{programId}")]
        public async Task<ActionResult<IEnumerable<LessonDto>>> GetLessonsByProgram(int programId)
        {
            var lessons = await _context.Lessons
                .Include(l => l.Materials)
                .Where(l => l.EducationalProgramId == programId)
                .OrderBy(l => l.OrderNumber)
                .Select(l => new LessonDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    EducationalProgramId = l.EducationalProgramId,
                    GroupId = l.GroupId,
                    OrderNumber = l.OrderNumber,
                    ScheduledAt = l.ScheduledAt,
                    DurationMinutes = l.DurationMinutes,
                    TeacherComment = l.TeacherComment,
                    CreatedAt = l.CreatedAt,
                    EducationalProgramName = l.EducationalProgram.Name,
                    GroupName = l.Group != null ? l.Group.Name : null
                })
                .ToListAsync();

            return lessons;
        }

        [HttpGet("Group/{groupId}")]
        public async Task<ActionResult<IEnumerable<LessonDto>>> GetLessonsByGroup(int groupId)
        {
            var lessons = await _context.Lessons
                .Include(l => l.Materials)
                .Include(l => l.Attendances)
                .Where(l => l.GroupId == groupId)
                .OrderBy(l => l.ScheduledAt)
                .Select(l => new LessonDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    EducationalProgramId = l.EducationalProgramId,
                    GroupId = l.GroupId,
                    OrderNumber = l.OrderNumber,
                    ScheduledAt = l.ScheduledAt,
                    DurationMinutes = l.DurationMinutes,
                    TeacherComment = l.TeacherComment,
                    CreatedAt = l.CreatedAt,
                    EducationalProgramName = l.EducationalProgram.Name,
                    GroupName = l.Group != null ? l.Group.Name : null
                })
                .ToListAsync();

            return lessons;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<LessonDto>> CreateLesson([FromBody] CreateLessonDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var program = await _context.EducationalPrograms.FindAsync(createDto.EducationalProgramId);
            if (program == null)
            {
                return BadRequest("Educational program not found");
            }

            if (createDto.GroupId.HasValue)
            {
                var group = await _context.Groups.FindAsync(createDto.GroupId.Value);
                if (group == null)
                {
                    return BadRequest("Group not found");
                }
            }

            var lesson = new Lesson
            {
                Title = createDto.Title,
                Description = createDto.Description,
                EducationalProgramId = createDto.EducationalProgramId,
                GroupId = createDto.GroupId,
                OrderNumber = createDto.OrderNumber,
                ScheduledAt = createDto.ScheduledAt,
                DurationMinutes = createDto.DurationMinutes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            // Возвращаем DTO с полной информацией
            var result = await _context.Lessons
                .Include(l => l.EducationalProgram)
                .Include(l => l.Group)
                .Where(l => l.Id == lesson.Id)
                .Select(l => new LessonDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    EducationalProgramId = l.EducationalProgramId,
                    GroupId = l.GroupId,
                    OrderNumber = l.OrderNumber,
                    ScheduledAt = l.ScheduledAt,
                    DurationMinutes = l.DurationMinutes,
                    TeacherComment = l.TeacherComment,
                    CreatedAt = l.CreatedAt,
                    EducationalProgramName = l.EducationalProgram.Name,
                    GroupName = l.Group != null ? l.Group.Name : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetLesson), new { id = lesson.Id }, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLesson(int id, [FromBody] UpdateLessonDto updateDto)
        {
            var existingLesson = await _context.Lessons.FindAsync(id);
            if (existingLesson == null)
            {
                return NotFound();
            }

            existingLesson.Title = updateDto.Title;
            existingLesson.Description = updateDto.Description;
            existingLesson.OrderNumber = updateDto.OrderNumber;
            existingLesson.ScheduledAt = updateDto.ScheduledAt;
            existingLesson.DurationMinutes = updateDto.DurationMinutes;
            existingLesson.TeacherComment = updateDto.TeacherComment;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LessonExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [Authorize(Roles = "Teacher")]
        [HttpPut("{id}/Comment")]
        public async Task<IActionResult> UpdateLessonComment(int id, [FromBody] string comment)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == lesson.GroupId && g.TeacherId == userId);
            if (group == null)
            {
                return Unauthorized("You can only comment on lessons in your groups");
            }

            lesson.TeacherComment = comment;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LessonExists(int id)
        {
            return _context.Lessons.Any(e => e.Id == id);
        }
    }
} 