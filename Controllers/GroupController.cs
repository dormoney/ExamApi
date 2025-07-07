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
    public class GroupController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GroupController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetGroups()
        {
            var groups = await _context.Groups
                .Include(g => g.EducationalProgram)
                .Include(g => g.Teacher)
                .Include(g => g.Students)
                .Select(g => new GroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    EducationalProgramId = g.EducationalProgramId,
                    TeacherId = g.TeacherId,
                    MaxStudents = g.MaxStudents,
                    IsOpen = g.IsOpen,
                    CreatedAt = g.CreatedAt,
                    EducationalProgramName = g.EducationalProgram.Name,
                    TeacherName = g.Teacher.Name,
                    StudentsCount = g.Students.Count
                })
                .ToListAsync();

            return groups;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GroupDetailDto>> GetGroup(int id)
        {
            var group = await _context.Groups
                .Include(g => g.EducationalProgram)
                .Include(g => g.Teacher)
                .Include(g => g.Students)
                .Include(g => g.Lessons)
                .Where(g => g.Id == id)
                .Select(g => new GroupDetailDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    EducationalProgramId = g.EducationalProgramId,
                    TeacherId = g.TeacherId,
                    MaxStudents = g.MaxStudents,
                    IsOpen = g.IsOpen,
                    CreatedAt = g.CreatedAt,
                    EducationalProgramName = g.EducationalProgram.Name,
                    TeacherName = g.Teacher.Name,
                    StudentsCount = g.Students.Count,
                    Students = g.Students.Select(s => new GroupStudentDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Email = s.Email
                    }).ToList(),
                    Lessons = g.Lessons.Select(l => new GroupLessonDto
                    {
                        Id = l.Id,
                        Title = l.Title,
                        ScheduledAt = l.ScheduledAt,
                        OrderNumber = l.OrderNumber
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (group == null)
            {
                return NotFound();
            }

            return group;
        }

        [Authorize]
        [HttpGet("MyGroups")]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetMyGroups()
        {
            var userId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<Group> query = _context.Groups
                .Include(g => g.EducationalProgram)
                .Include(g => g.Teacher)
                .Include(g => g.Students);

            if (userRole == "Teacher")
            {
                query = query.Where(g => g.TeacherId == userId);
            }
            else if (userRole == "Student")
            {
                query = query.Where(g => g.Students.Any(s => s.Id == userId));
            }
            else
            {
                return Unauthorized();
            }

            var groups = await query
                .Select(g => new GroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    EducationalProgramId = g.EducationalProgramId,
                    TeacherId = g.TeacherId,
                    MaxStudents = g.MaxStudents,
                    IsOpen = g.IsOpen,
                    CreatedAt = g.CreatedAt,
                    EducationalProgramName = g.EducationalProgram.Name,
                    TeacherName = g.Teacher.Name,
                    StudentsCount = g.Students.Count
                })
                    .ToListAsync();

            return groups;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<GroupDto>> CreateGroup([FromBody] CreateGroupDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Id == createDto.TeacherId && u.Role == "Teacher");
            if (teacher == null)
            {
                return BadRequest("Teacher not found or user is not a teacher");
            }

            var program = await _context.EducationalPrograms.FindAsync(createDto.EducationalProgramId);
            if (program == null)
            {
                return BadRequest("Educational program not found");
            }

            var group = new Group
            {
                Name = createDto.Name,
                Description = createDto.Description,
                EducationalProgramId = createDto.EducationalProgramId,
                TeacherId = createDto.TeacherId,
                MaxStudents = createDto.MaxStudents,
                IsOpen = createDto.IsOpen,
                CreatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            // Возвращаем DTO с полной информацией
            var result = await _context.Groups
                .Include(g => g.EducationalProgram)
                .Include(g => g.Teacher)
                .Include(g => g.Students)
                .Where(g => g.Id == group.Id)
                .Select(g => new GroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    EducationalProgramId = g.EducationalProgramId,
                    TeacherId = g.TeacherId,
                    MaxStudents = g.MaxStudents,
                    IsOpen = g.IsOpen,
                    CreatedAt = g.CreatedAt,
                    EducationalProgramName = g.EducationalProgram.Name,
                    TeacherName = g.Teacher.Name,
                    StudentsCount = g.Students.Count
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(int id, [FromBody] UpdateGroupDto updateDto)
        {
            var existingGroup = await _context.Groups.FindAsync(id);
            if (existingGroup == null)
            {
                return NotFound();
            }

            existingGroup.Name = updateDto.Name;
            existingGroup.Description = updateDto.Description;
            existingGroup.MaxStudents = updateDto.MaxStudents;
            existingGroup.IsOpen = updateDto.IsOpen;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupExists(id))
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

        [Authorize(Roles = "Student")]
        [HttpPost("{id}/Join")]
        public async Task<IActionResult> JoinGroup(int id)
        {
            var userId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            
            var group = await _context.Groups
                .Include(g => g.Students)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound("Group not found");
            }

            if (!group.IsOpen)
            {
                return BadRequest("Group is not open for joining");
            }

            if (group.Students.Count >= group.MaxStudents)
            {
                return BadRequest("Group is full");
            }

            var student = await _context.Users.FindAsync(userId);
            if (student == null)
            {
                return NotFound("Student not found");
            }

            if (group.Students.Any(s => s.Id == userId))
            {
                return BadRequest("Student is already in this group");
            }

            group.Students.Add(student);
            await _context.SaveChangesAsync();

            return Ok("Successfully joined the group");
        }

        [Authorize(Roles = "Student")]
        [HttpPost("{id}/Leave")]
        public async Task<IActionResult> LeaveGroup(int id)
        {
            var userId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            
            var group = await _context.Groups
                .Include(g => g.Students)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound("Group not found");
            }

            var student = group.Students.FirstOrDefault(s => s.Id == userId);
            if (student == null)
            {
                return BadRequest("Student is not in this group");
            }

            group.Students.Remove(student);
            await _context.SaveChangesAsync();

            return Ok("Successfully left the group");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GroupExists(int id)
        {
            return _context.Groups.Any(e => e.Id == id);
        }
    }
} 