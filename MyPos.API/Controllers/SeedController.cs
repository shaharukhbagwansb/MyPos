using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPos.API.Application.Helpers;
using MyPos.API.Domain.Entities;
using MyPos.API.Infrastructure.Data;

namespace MyPos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SeedController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin()
        {
            if (await _context.Users.AnyAsync())
            {
                return BadRequest(new { message = "Users already exist." });
            }

            var client = await _context.Clients.FirstOrDefaultAsync();
            if (client == null)
                return BadRequest(new { message = "No Client found. Pehle POS_Script.sql run karo." });

            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "ADMIN");
            if (adminRole == null)
                return BadRequest(new { message = "ADMIN role nahi mila." });

            var admin = new User
            {
                ClientID = client.ClientID,
                RoleId = adminRole.RoleId,
                UserName = "admin",
                PasswordHash = PasswordHelper.Encrypt("admin@123"),
                FullName = "Shop Owner",
                Mobile = "9999999999",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(admin);

            // Shop Setting
            if (!await _context.ShopSettings.AnyAsync(s => s.ClientID == client.ClientID))
            {
                _context.ShopSettings.Add(new ShopSetting
                {
                    ClientID = client.ClientID,
                    ShopName = client.ClientName,
                    City = "Ahmedabad",
                    State = "Gujarat",
                    InvoicePrefix = "INV",
                    NextInvoiceNumber = 1,
                    CurrencyCode = "INR",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Admin created successfully",
                username = "admin",
                password = "Admin@123"
            });
        }
    }
}