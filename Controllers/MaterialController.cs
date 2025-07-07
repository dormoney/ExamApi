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
    public class MaterialController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaterialController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetMaterials()
        {
            var materials = await _context.Materials
                .Include(m => m.Lesson)
                .Select(m => new MaterialDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    LessonId = m.LessonId,
                    Content = m.Content,
                    Link = m.Link,
                    Type = m.Type,
                    OrderNumber = m.OrderNumber,
                    CreatedAt = m.CreatedAt,
                    LessonTitle = m.Lesson.Title
                })
                .ToListAsync();

            return materials;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MaterialDto>> GetMaterial(int id)
        {
            var material = await _context.Materials
                .Include(m => m.Lesson)
                .Where(m => m.Id == id)
                .Select(m => new MaterialDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    LessonId = m.LessonId,
                    Content = m.Content,
                    Link = m.Link,
                    Type = m.Type,
                    OrderNumber = m.OrderNumber,
                    CreatedAt = m.CreatedAt,
                    LessonTitle = m.Lesson.Title
                })
                .FirstOrDefaultAsync();

            if (material == null)
            {
                return NotFound();
            }

            return material;
        }

        [HttpGet("Lesson/{lessonId}")]
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetMaterialsByLesson(int lessonId)
        {
            var materials = await _context.Materials
                .Include(m => m.Lesson)
                .Where(m => m.LessonId == lessonId)
                .OrderBy(m => m.OrderNumber)
                .Select(m => new MaterialDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    LessonId = m.LessonId,
                    Content = m.Content,
                    Link = m.Link,
                    Type = m.Type,
                    OrderNumber = m.OrderNumber,
                    CreatedAt = m.CreatedAt,
                    LessonTitle = m.Lesson.Title
                })
                .ToListAsync();

            return materials;
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost]
        public async Task<ActionResult<MaterialDto>> CreateMaterial([FromBody] CreateMaterialDto createDto)
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
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "Teacher")
            {
                if (lesson.Group?.TeacherId != userId)
                {
                    return Unauthorized("You can only add materials to lessons in your groups");
                }
            }

            var material = new Material
            {
                Title = createDto.Title,
                Description = createDto.Description,
                LessonId = createDto.LessonId,
                Content = createDto.Content,
                Link = createDto.Link,
                Type = createDto.Type,
                OrderNumber = createDto.OrderNumber,
                CreatedAt = DateTime.UtcNow
            };

            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            // Возвращаем DTO с полной информацией
            var result = await _context.Materials
                .Include(m => m.Lesson)
                .Where(m => m.Id == material.Id)
                .Select(m => new MaterialDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    LessonId = m.LessonId,
                    Content = m.Content,
                    Link = m.Link,
                    Type = m.Type,
                    OrderNumber = m.OrderNumber,
                    CreatedAt = m.CreatedAt,
                    LessonTitle = m.Lesson.Title
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetMaterial), new { id = material.Id }, result);
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMaterial(int id, [FromBody] UpdateMaterialDto updateDto)
        {
            var existingMaterial = await _context.Materials
                .Include(m => m.Lesson)
                    .ThenInclude(l => l.Group)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (existingMaterial == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "Teacher")
            {
                if (existingMaterial.Lesson?.Group?.TeacherId != userId)
                {
                    return Unauthorized("You can only update materials for lessons in your groups");
                }
            }

            existingMaterial.Title = updateDto.Title;
            existingMaterial.Description = updateDto.Description;
            existingMaterial.Content = updateDto.Content;
            existingMaterial.Link = updateDto.Link;
            existingMaterial.Type = updateDto.Type;
            existingMaterial.OrderNumber = updateDto.OrderNumber;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialExists(id))
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

        [Authorize(Roles = "Teacher,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var material = await _context.Materials
                .Include(m => m.Lesson)
                    .ThenInclude(l => l.Group)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "Teacher")
            {
                if (material.Lesson?.Group?.TeacherId != userId)
                {
                    return Unauthorized("You can only delete materials for lessons in your groups");
                }
            }

            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MaterialExists(int id)
        {
            return _context.Materials.Any(e => e.Id == id);
        }
    }
} 