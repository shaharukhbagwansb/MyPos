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
    public class RegisterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetClientId() => int.Parse(User.FindFirst("ClientID")?.Value ?? "0");
        private string GetRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        private int GetUserId() => int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        // ==================== SUPERADMIN: Naya Shop + Owner ====================
        [HttpPost("shop")]
        [Authorize(Roles = "SUPERADMIN")]
        public async Task<IActionResult> RegisterShop([FromBody] RegisterShopRequest request)
        {
            // Username already exists?
            if (await _context.Users.AnyAsync(u => u.UserName == request.OwnerUserName))
                return BadRequest(new { message = "Username already exists" });

            // 1. Naya Client (Shop) banao
            var client = new Client
            {
                ClientName = request.ShopName.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            // 2. Admin Role nikalo
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "ADMIN");
            if (adminRole == null)
                return BadRequest(new { message = "ADMIN role not found" });

            // 3. Shop Owner (Admin) banao
            var owner = new User
            {
                ClientID = client.ClientID,
                RoleId = adminRole.RoleId,
                UserName = request.OwnerUserName.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.OwnerPassword),
                FullName = request.OwnerFullName.Trim(),
                Mobile = request.OwnerMobile,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(owner);

            // 4. Shop Settings
            _context.ShopSettings.Add(new ShopSetting
            {
                ClientID = client.ClientID,
                ShopName = request.ShopName.Trim(),
                City = request.City,
                State = request.State,
                Mobile = request.Mobile,
                GSTIN = request.GSTIN,
                InvoicePrefix = "INV",
                NextInvoiceNumber = 1,
                CurrencyCode = "INR",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Shop and Owner registered successfully",
                clientId = client.ClientID,
                shopName = client.ClientName,
                ownerUserName = owner.UserName
            });
        }

        // ==================== ADMIN: Staff Register ====================
        [HttpPost("staff")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterStaffRequest request)
        {
            var clientId = GetClientId();

            // Username already exists?
            if (await _context.Users.AnyAsync(u => u.UserName == request.UserName))
                return BadRequest(new { message = "Username already exists" });

            // Role check
            var allowedRoles = new[] { "SALES", "PURCHASE", "ACCOUNT" };
            if (!allowedRoles.Contains(request.RoleName.ToUpper()))
                return BadRequest(new { message = "Role must be SALES, PURCHASE or ACCOUNT" });

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == request.RoleName.ToUpper());
            if (role == null)
                return BadRequest(new { message = "Role not found" });

            var staff = new User
            {
                ClientID = clientId,
                RoleId = role.RoleId,
                UserName = request.UserName.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName.Trim(),
                Mobile = request.Mobile,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(staff);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Staff registered successfully",
                userId = staff.UserId,
                userName = staff.UserName,
                role = role.RoleName
            });
        }
    }
}