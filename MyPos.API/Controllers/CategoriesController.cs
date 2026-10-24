using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPos.API.Application.DTOs;
using MyPos.API.Domain.Entities;
using MyPos.API.Infrastructure.Data;
using System.Security.Claims;

namespace MyPos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetClientId()
        {
            return int.Parse(User.FindFirst("ClientID")?.Value ?? "0");
        }

        // GET All
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var clientId = GetClientId();

            var list = await _context.Categories
                .Where(c => c.ClientID == clientId && c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Select(c => new
                {
                    c.CategoryId,
                    c.CategoryName,
                    c.Description,
                    c.IsActive
                })
                .ToListAsync();

            return Ok(list);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            var clientId = GetClientId();

            if (string.IsNullOrWhiteSpace(request.CategoryName))
                return BadRequest(new { message = "CategoryName required" });

            var name = request.CategoryName.Trim();

            var exists = await _context.Categories
                .AnyAsync(c => c.ClientID == clientId && c.CategoryName == name && c.IsActive);

            if (exists)
                return BadRequest(new { message = "Category already exists" });

            var category = new Category
            {
                ClientID = clientId,
                CategoryName = name,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                categoryId = category.CategoryId,
                categoryName = category.CategoryName
            });
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryRequest request)
        {
            var clientId = GetClientId();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.ClientID == clientId);

            if (category == null)
                return NotFound(new { message = "Category not found" });

            category.CategoryName = request.CategoryName.Trim();
            category.Description = request.Description;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Updated successfully" });
        }

        // DELETE (Soft)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var clientId = GetClientId();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.ClientID == clientId);

            if (category == null)
                return NotFound(new { message = "Category not found" });

            category.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }
    }
}