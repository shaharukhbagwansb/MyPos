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
    public class BrandsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BrandsController(ApplicationDbContext context)
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

            var list = await _context.Brands
                .Where(c => c.ClientID == clientId && c.IsActive)
                .OrderBy(c => c.BrandName)
                .Select(c => new
                {
                    c.BrandId,
                    c.BrandName,
                    c.IsActive
                })
                .ToListAsync();

            return Ok(list);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBrandRequest request)
        {
            var clientId = GetClientId();

            if (string.IsNullOrWhiteSpace(request.BrandName))
                return BadRequest(new { message = "Brand Name is Required" });

            var name = request.BrandName.Trim();

            var exists = await _context.Brands
                .AnyAsync(c => c.ClientID == clientId && c.BrandName == name && c.IsActive);

            if (exists)
                return BadRequest(new { message = "Brand is already exists" });

            var brand = new Brand
            {
                ClientID = clientId,
                 BrandName = name,
                
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                brandId = brand.BrandId,
                brandName = brand.BrandName
            });
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateBrandRequest request)
        {
            var clientId = GetClientId();

            var brand = await _context.Brands
                .FirstOrDefaultAsync(c => c.BrandId == id && c.ClientID == clientId);

            if (brand == null)
                return NotFound(new { message = "Brand is not found" });

            brand.BrandName = request.BrandName.Trim();
            

            await _context.SaveChangesAsync();
            return Ok(new { message = "Updated successfully" });
        }
        // DELETE (Soft)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var clientId = GetClientId();

            var brand = await _context.Brands
                .FirstOrDefaultAsync(c => c.BrandId == id && c.ClientID == clientId);

            if (brand == null)
                return NotFound(new { message = " Brand is not found" });

            brand.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }

    }
}
