using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPos.API.Application.DTOs;
using MyPos.API.Application.Helpers;
using MyPos.API.Domain.Entities;
using MyPos.API.Infrastructure.Data;

namespace MyPos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public ForgotPasswordController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ==================== 1. OTP BHEJO ====================
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName))
                return BadRequest(new { message = "Username required" });

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == request.UserName && u.IsActive);

            if (user == null)
                return NotFound(new { message = "User not found" });

            if (string.IsNullOrEmpty(user.Email))
                return BadRequest(new { message = "Email not registered for this user. Contact SuperAdmin." });

            // 6 digit OTP generate
            var otp = new Random().Next(100000, 999999).ToString();

            // Purane unused OTP expire kar do
            var oldOtps = await _context.PasswordResetOtps
                .Where(o => o.UserId == user.UserId && !o.IsUsed)
                .ToListAsync();

            foreach (var old in oldOtps)
                old.IsUsed = true;

            // Naya OTP save
            var otpEntity = new PasswordResetOtp
            {
                UserId = user.UserId,
                Email = user.Email,
                Otp = otp,
                ExpiryTime = DateTime.UtcNow.AddMinutes(10), // 10 minute valid
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.PasswordResetOtps.Add(otpEntity);
            await _context.SaveChangesAsync();

            // Email bhejo
            try
            {
                await SendEmailAsync(user.Email, "Password Reset OTP - My POS",
                    $"Your OTP is: <b>{otp}</b><br/><br/>This OTP is valid for 10 minutes.<br/><br/>If you did not request this, please ignore.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "OTP generated but email failed to send", error = ex.Message });
            }

            return Ok(new
            {
                message = "OTP sent successfully to your email",
                email = MaskEmail(user.Email)
            });
        }

        // ==================== 2. OTP VERIFY ====================
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == request.UserName && u.IsActive);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var otpRecord = await _context.PasswordResetOtps
                .Where(o => o.UserId == user.UserId &&
                            o.Otp == request.Otp &&
                            !o.IsUsed &&
                            o.ExpiryTime > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
                return BadRequest(new { message = "Invalid or expired OTP" });

            return Ok(new { message = "OTP verified successfully" });
        }

        // ==================== 3. NEW PASSWORD SET ====================
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                return BadRequest(new { message = "Password must be at least 6 characters" });

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == request.UserName && u.IsActive);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var otpRecord = await _context.PasswordResetOtps
                .Where(o => o.UserId == user.UserId &&
                            o.Otp == request.Otp &&
                            !o.IsUsed &&
                            o.ExpiryTime > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
                return BadRequest(new { message = "Invalid or expired OTP" });

            // Password update (Encrypt karke)
            user.PasswordHash = PasswordHelper.Encrypt(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // OTP used mark karo
            otpRecord.IsUsed = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successfully. Please login with new password." });
        }

        // ==================== HELPERS ====================
        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailSettings = _config.GetSection("EmailSettings");
            var fromEmail = emailSettings["FromEmail"];
            var fromPassword = emailSettings["FromPassword"];
            var smtpHost = emailSettings["SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(fromEmail, fromPassword)
            };

            var mail = new MailMessage(fromEmail!, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mail);
        }

        private static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                return email;

            var parts = email.Split('@');
            var name = parts[0];
            var domain = parts[1];

            if (name.Length <= 2)
                return $"{name[0]}***@{domain}";

            return $"{name.Substring(0, 2)}***@{domain}";
        }
    }
}