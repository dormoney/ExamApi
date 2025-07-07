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
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendances()
        {
            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Lesson)
                .Include(a => a.MarkedByTeacher)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    LessonId = a.LessonId,
                    IsPresent = a.IsPresent,
                    MarkedAt = a.MarkedAt,
                    MarkedByTeacherId = a.MarkedByTeacherId,
                    StudentName = a.Student.Name,
                    LessonTitle = a.Lesson.Title,
                    MarkedByTeacherName = a.MarkedByTeacher != null ? a.MarkedByTeacher.Name : null
                })
                .ToListAsync();

            return attendances;
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<AttendanceDto>> GetAttendance(int id)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Lesson)
                .Include(a => a.MarkedByTeacher)
                .Where(a => a.Id == id)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    LessonId = a.LessonId,
                    IsPresent = a.IsPresent,
                    MarkedAt = a.MarkedAt,
                    MarkedByTeacherId = a.MarkedByTeacherId,
                    StudentName = a.Student.Name,
                    LessonTitle = a.Lesson.Title,
                    MarkedByTeacherName = a.MarkedByTeacher != null ? a.MarkedByTeacher.Name : null
                })
                .FirstOrDefaultAsync();

            if (attendance == null)
            {
                return NotFound();
            }

            return attendance;
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpGet("Lesson/{lessonId}")]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendancesByLesson(int lessonId)
        {
            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.MarkedByTeacher)
                .Where(a => a.LessonId == lessonId)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    LessonId = a.LessonId,
                    IsPresent = a.IsPresent,
                    MarkedAt = a.MarkedAt,
                    MarkedByTeacherId = a.MarkedByTeacherId,
                    StudentName = a.Student.Name,
                    LessonTitle = a.Lesson.Title,
                    MarkedByTeacherName = a.MarkedByTeacher != null ? a.MarkedByTeacher.Name : null
                })
                .ToListAsync();

            return attendances;
        }

        [Authorize]
        [HttpGet("Student/{studentId}")]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendancesByStudent(int studentId)
        {
            var userId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "Student" && userId != studentId)
            {
                return Unauthorized();
            }

            IQueryable<Attendance> query = _context.Attendances
                .Include(a => a.Lesson)
                .Include(a => a.MarkedByTeacher)
                .Where(a => a.StudentId == studentId);

            if (userRole == "Teacher")
            {
                var teacherGroups = await _context.Groups
                    .Where(g => g.TeacherId == userId)
                    .Select(g => g.Id)
                    .ToListAsync();

                var lessonIds = await _context.Lessons
                    .Where(l => teacherGroups.Contains(l.GroupId.Value))
                    .Select(l => l.Id)
                    .ToListAsync();

                query = query.Where(a => lessonIds.Contains(a.LessonId));
            }

            var attendances = await query
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    LessonId = a.LessonId,
                    IsPresent = a.IsPresent,
                    MarkedAt = a.MarkedAt,
                    MarkedByTeacherId = a.MarkedByTeacherId,
                    StudentName = a.Student.Name,
                    LessonTitle = a.Lesson.Title,
                    MarkedByTeacherName = a.MarkedByTeacher != null ? a.MarkedByTeacher.Name : null
                })
                    .ToListAsync();

            return attendances;
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public async Task<ActionResult<AttendanceDto>> CreateAttendance([FromBody] CreateAttendanceDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var lesson = await _context.Lessons
                .Include(l => l.Group)
                .FirstOrDefaultAsync(l => l.Id == createDto.LessonId);
            if (lesson == null)
            {
                return BadRequest("Lesson not found");
            }

            var userId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            if (lesson.Group?.TeacherId != userId)
            {
                return Unauthorized("You can only mark attendance for lessons in your groups");
            }

            var studentInGroup = await _context.Groups
                .Include(g => g.Students)
                .Where(g => g.Id == lesson.GroupId)
                .AnyAsync(g => g.Students.Any(s => s.Id == createDto.StudentId));

            if (!studentInGroup)
            {
                return BadRequest("Student is not in this group");
            }

            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == createDto.StudentId && a.LessonId == createDto.LessonId);

            if (existingAttendance != null)
            {
                return BadRequest("Attendance record already exists for this student and lesson");
            }

            var attendance = new Attendance
            {
                StudentId = createDto.StudentId,
                LessonId = createDto.LessonId,
                IsPresent = createDto.IsPresent,
                MarkedByTeacherId = userId,
                MarkedAt = DateTime.UtcNow
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            // Возвращаем DTO с полной информацией
            var result = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Lesson)
                .Include(a => a.MarkedByTeacher)
                .Where(a => a.Id == attendance.Id)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    LessonId = a.LessonId,
                    IsPresent = a.IsPresent,
                    MarkedAt = a.MarkedAt,
                    MarkedByTeacherId = a.MarkedByTeacherId,
                    StudentName = a.Student.Name,
                    LessonTitle = a.Lesson.Title,
                    MarkedByTeacherName = a.MarkedByTeacher != null ? a.MarkedByTeacher.Name : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetAttendance), new { id = attendance.Id }, result);
        }

        [Authorize(Roles = "Teacher")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttendance(int id, [FromBody] UpdateAttendanceDto updateDto)
        {
            var existingAttendance = await _context.Attendances
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Group)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingAttendance == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            if (existingAttendance.Lesson?.Group?.TeacherId != userId)
            {
                return Unauthorized("You can only update attendance for lessons in your groups");
            }

            existingAttendance.IsPresent = updateDto.IsPresent;
            existingAttendance.MarkedByTeacherId = userId;
            existingAttendance.MarkedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AttendanceExists(id))
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
        [HttpPost("Bulk")]
        public async Task<IActionResult> CreateBulkAttendance([FromBody] CreateBulkAttendanceDto bulkDto)
        {
            if (!bulkDto.Students.Any())
            {
                return BadRequest("No attendance records provided");
            }

            var lesson = await _context.Lessons
                .Include(l => l.Group)
                .FirstOrDefaultAsync(l => l.Id == bulkDto.LessonId);
            if (lesson == null)
            {
                return BadRequest("Lesson not found");
            }

            var userId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            if (lesson.Group?.TeacherId != userId)
            {
                return Unauthorized("You can only mark attendance for lessons in your groups");
            }

            var studentIds = bulkDto.Students.Select(s => s.StudentId).ToList();
            var studentInGroup = await _context.Groups
                .Include(g => g.Students)
                .Where(g => g.Id == lesson.GroupId)
                .AnyAsync(g => g.Students.Any(s => studentIds.Contains(s.Id)));

            if (!studentInGroup)
            {
                return BadRequest("Some students are not in this group");
            }

            // Удаляем существующие записи для этого урока
            var existingAttendances = await _context.Attendances
                .Where(a => a.LessonId == bulkDto.LessonId)
                .ToListAsync();
            _context.Attendances.RemoveRange(existingAttendances);

            // Создаем новые записи
            foreach (var studentAttendance in bulkDto.Students)
            {
                var attendance = new Attendance
            {
                    StudentId = studentAttendance.StudentId,
                    LessonId = bulkDto.LessonId,
                    IsPresent = studentAttendance.IsPresent,
                    MarkedByTeacherId = userId,
                    MarkedAt = DateTime.UtcNow
                };
                _context.Attendances.Add(attendance);
            }

            await _context.SaveChangesAsync();
            return Ok("Bulk attendance created successfully");
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Group)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "Teacher" && attendance.Lesson?.Group?.TeacherId != userId)
            {
                return Unauthorized("You can only delete attendance for lessons in your groups");
            }

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AttendanceExists(int id)
        {
            return _context.Attendances.Any(e => e.Id == id);
        }
    }
} 