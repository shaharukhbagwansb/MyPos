using System.ComponentModel.DataAnnotations;

namespace MyPos.API.Application.DTOs
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
    }

    public class VerifyOtpRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Otp { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Otp { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}