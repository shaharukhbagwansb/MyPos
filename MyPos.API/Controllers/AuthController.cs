using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyPos.API.Application.DTOs;
using MyPos.API.Application.Helpers;
using MyPos.API.Domain.Entities;
using MyPos.API.Infrastructure.Data;
using MyPos.API.Application.Helpers;
namespace MyPos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ==================== LOGIN ====================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Username and Password required" });

            if (string.IsNullOrWhiteSpace(request.DeviceId) || string.IsNullOrWhiteSpace(request.Platform))
                return BadRequest(new { message = "DeviceId and Platform required" });

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Client)
                .FirstOrDefaultAsync(u =>
                    u.UserName == request.UserName &&
                    u.IsActive &&
                    u.Client.IsActive);

            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            // Password check
            string decryptedPassword = PasswordHelper.Decrypt(user.PasswordHash);
            bool isPasswordValid = decryptedPassword == request.Password;
            if (!isPasswordValid)
                return Unauthorized(new { message = "Invalid username or password" });

            // ========== DEVICE BINDING LOGIC ==========
            bool isSuperAdmin = user.Role.RoleName.ToUpper() == "SUPERADMIN";

            // SuperAdmin ko device binding se free rakhte hain (optional)
            if (!isSuperAdmin)
            {
                // Pehli baar login → device register karo
                if (string.IsNullOrEmpty(user.RegisteredDeviceId))
                {
                    user.RegisteredDeviceId = request.DeviceId;
                    user.RegisteredPlatform = request.Platform;
                    user.DeviceRegisteredAt = DateTime.UtcNow;
                }
                else
                {
                    // Device match nahi kar raha
                    if (user.RegisteredDeviceId != request.DeviceId)
                    {
                        // Check if already pending request hai
                        var existingRequest = await _context.DeviceApprovalRequests
                            .FirstOrDefaultAsync(r =>
                                r.UserId == user.UserId &&
                                r.DeviceId == request.DeviceId &&
                                r.Status == "PENDING");

                        if (existingRequest == null)
                        {
                            // Naya approval request banao
                            _context.DeviceApprovalRequests.Add(new DeviceApprovalRequest
                            {
                                ClientID = user.ClientID,
                                UserId = user.UserId,
                                DeviceId = request.DeviceId,
                                Platform = request.Platform,
                                RequestedAt = DateTime.UtcNow,
                                Status = "PENDING"
                            });
                            await _context.SaveChangesAsync();
                        }

                        return Unauthorized(new
                        {
                            message = "This device is not registered. Please wait for SuperAdmin approval.",
                            code = "DEVICE_NOT_APPROVED"
                        });
                    }
                }
            }

            // ========== TOKEN GENERATE ==========
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();
            var accessExpiry = DateTime.UtcNow.AddMinutes(GetAccessTokenMinutes());
            var refreshExpiry = DateTime.UtcNow.AddDays(GetRefreshTokenDays());

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = refreshExpiry;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var shop = await _context.ShopSettings
                .FirstOrDefaultAsync(s => s.ClientID == user.ClientID);

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = accessExpiry,
                RefreshTokenExpiry = refreshExpiry,
                User = new UserInfo
                {
                    UserId = user.UserId,
                    ClientID = user.ClientID,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Mobile = user.Mobile,
                    RoleId = user.RoleId,
                    RoleName = user.Role.RoleName
                },
                Shop = shop == null ? null : new ShopInfo
                {
                    ShopSettingId = shop.ShopSettingId,
                    ShopName = shop.ShopName,
                    Address = $"{shop.AddressLine1} {shop.AddressLine2}".Trim(),
                    City = shop.City,
                    GSTIN = shop.GSTIN,
                    CurrencyCode = shop.CurrencyCode,
                    InvoicePrefix = shop.InvoicePrefix
                }
            };

            return Ok(response);
        }

        // ==================== AUTO LOGIN ====================
        [HttpPost("auto-login")]
        public async Task<IActionResult> AutoLogin([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { message = "RefreshToken required" });

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Client)
                .FirstOrDefaultAsync(u =>
                    u.RefreshToken == request.RefreshToken &&
                    u.IsActive &&
                    u.Client.IsActive);

            if (user == null || user.RefreshTokenExpiry == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                return Ok(new AutoLoginResponse
                {
                    IsValid = false,
                    Message = "Session expired. Please login again."
                });
            }

            // Naye tokens
            var accessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();
            var accessExpiry = DateTime.UtcNow.AddMinutes(GetAccessTokenMinutes());
            var refreshExpiry = DateTime.UtcNow.AddDays(GetRefreshTokenDays());

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = refreshExpiry;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var shop = await _context.ShopSettings
                .FirstOrDefaultAsync(s => s.ClientID == user.ClientID);

            var loginData = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiry = accessExpiry,
                RefreshTokenExpiry = refreshExpiry,
                User = new UserInfo
                {
                    UserId = user.UserId,
                    ClientID = user.ClientID,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Mobile = user.Mobile,
                    RoleId = user.RoleId,
                    RoleName = user.Role.RoleName
                },
                Shop = shop == null ? null : new ShopInfo
                {
                    ShopSettingId = shop.ShopSettingId,
                    ShopName = shop.ShopName,
                    Address = $"{shop.AddressLine1} {shop.AddressLine2}".Trim(),
                    City = shop.City,
                    GSTIN = shop.GSTIN,
                    CurrencyCode = shop.CurrencyCode,
                    InvoicePrefix = shop.InvoicePrefix
                }
            };

            return Ok(new AutoLoginResponse
            {
                IsValid = true,
                LoginData = loginData
            });
        }

        // ==================== LOGOUT ====================
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Logged out successfully" });
        }

        // ==================== HELPERS ====================
        private string GenerateAccessToken(User user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("ClientID", user.ClientID.ToString()),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
                new Claim("RoleId", user.RoleId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(GetAccessTokenMinutes()),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private int GetAccessTokenMinutes()
        {
            return int.Parse(_config["JwtSettings:AccessTokenExpirationMinutes"] ?? "480");
        }

        private int GetRefreshTokenDays()
        {
            return int.Parse(_config["JwtSettings:RefreshTokenExpirationDays"] ?? "30");
        }
    }
}