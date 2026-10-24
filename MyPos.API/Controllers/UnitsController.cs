using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPos.API.Application.DTOs;
using MyPos.API.Domain.Entities;
using MyPos.API.Infrastructure.Data;
using System.Security.Cryptography.X509Certificates;

namespace MyPos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UnitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UnitsController(ApplicationDbContext context)
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

            var list = await _context.Units
                .Where(u => u.ClientID == clientId && u.IsActive)
                .OrderBy(u => u.UnitName)
                .Select(u => new
                {
                    u.UnitId,
                    u.UnitName,
                    u.IsActive
                })
                .ToListAsync();

            return Ok(list);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateUnitRequest request)
        {
            var clientId = GetClientId();

            if (string.IsNullOrWhiteSpace(request.UnitName))
                return BadRequest(new
                {
                    message = "Unit Name is Required"
                });
            var name = request.UnitName.Trim();

            if (string.IsNullOrWhiteSpace(request.ShortName))
                return BadRequest(new
                {
                    message = "Unit Name is Required"
                });

            var shortname = request.ShortName.Trim();

            var exists = await _context.Units
                .AnyAsync(u =>
                    u.ClientID == clientId &&
                    u.UnitName == name &&
                    u.ShortName == shortname &&
                    u.IsActive);

            if (exists)
                return BadRequest(new
                {
                    message = "Unit already exists"
                });

            var unit = new Unit
            {
                ClientID = clientId,
                UnitName = name,
                IsActive = true,
                ShortName = shortname
            };

            _context.Units.Add(unit);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                unitId = unit.UnitId,
                unitName = unit.UnitName,
                shortName = unit.ShortName
            });
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CreateUnitRequest request)
        {
            var clientId = GetClientId();

            if (string.IsNullOrWhiteSpace(request.UnitName))
                return BadRequest(new
                {
                    message = "Unit Name is Required"
                });

            var unit = await _context.Units
                .FirstOrDefaultAsync(u =>
                    u.UnitId == id &&
                    u.ClientID == clientId);

            if (unit == null)
                return NotFound(new
                {
                    message = "Unit not found"
                });

            var name = request.UnitName.Trim();
            var shortname = request.UnitName.Trim();

            var exists = await _context.Units
                .AnyAsync(u =>
                    u.ClientID == clientId &&
                    u.UnitId != id &&
                    u.UnitName == name &&
                    u.ShortName == shortname &&
                    u.IsActive);

            if (exists)
                return BadRequest(new
                {
                    message = "Unit already exists"
                });

            unit.UnitName = name;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Updated successfully"
            });
        }

        // DELETE (Soft)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var clientId = GetClientId();

            var unit = await _context.Units
                .FirstOrDefaultAsync(u =>
                    u.UnitId == id &&
                    u.ClientID == clientId);

            if (unit == null)
                return NotFound(new
                {
                    message = "Unit not found"
                });

            unit.IsActive = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Deleted successfully"
            });
        }
    }
}