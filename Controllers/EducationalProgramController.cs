using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExamApi.Data;
using ExamApi.Models;
using ExamApi.Models.DTOs;

namespace ExamApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EducationalProgramController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EducationalProgramController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EducationalProgramDto>>> GetEducationalPrograms()
        {
            var programs = await _context.EducationalPrograms
                .Where(ep => ep.IsActive)
                .Select(ep => new EducationalProgramDto
                {
                    Id = ep.Id,
                    Name = ep.Name,
                    Description = ep.Description,
                    CreatedAt = ep.CreatedAt,
                    IsActive = ep.IsActive
                })
                .ToListAsync();

            return programs;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EducationalProgramDto>> GetEducationalProgram(int id)
        {
            var educationalProgram = await _context.EducationalPrograms
                .Where(ep => ep.Id == id && ep.IsActive)
                .Select(ep => new EducationalProgramDto
                {
                    Id = ep.Id,
                    Name = ep.Name,
                    Description = ep.Description,
                    CreatedAt = ep.CreatedAt,
                    IsActive = ep.IsActive
                })
                .FirstOrDefaultAsync();

            if (educationalProgram == null)
            {
                return NotFound();
            }

            return educationalProgram;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<EducationalProgramDto>> CreateEducationalProgram([FromBody] CreateEducationalProgramDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var educationalProgram = new EducationalProgram
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive
            };

            _context.EducationalPrograms.Add(educationalProgram);
            await _context.SaveChangesAsync();

            var result = new EducationalProgramDto
            {
                Id = educationalProgram.Id,
                Name = educationalProgram.Name,
                Description = educationalProgram.Description,
                CreatedAt = educationalProgram.CreatedAt,
                IsActive = educationalProgram.IsActive
            };

            return CreatedAtAction(nameof(GetEducationalProgram), new { id = educationalProgram.Id }, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEducationalProgram(int id, [FromBody] UpdateEducationalProgramDto dto)
        {
            var existingProgram = await _context.EducationalPrograms.FindAsync(id);
            if (existingProgram == null)
            {
                return NotFound();
            }

            existingProgram.Name = dto.Name;
            existingProgram.Description = dto.Description;
            existingProgram.IsActive = dto.IsActive;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EducationalProgramExists(id))
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

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEducationalProgram(int id)
        {
            var educationalProgram = await _context.EducationalPrograms.FindAsync(id);
            if (educationalProgram == null)
            {
                return NotFound();
            }

            educationalProgram.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EducationalProgramExists(int id)
        {
            return _context.EducationalPrograms.Any(e => e.Id == id);
        }
    }
} 