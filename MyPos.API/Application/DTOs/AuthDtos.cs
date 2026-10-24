using System.ComponentModel.DataAnnotations;

namespace MyPos.API.Application.DTOs
{
    public class LoginRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        // Device Binding ke liye
        [Required]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        public string Platform { get; set; } = string.Empty; // "android" ya "ios"
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiry { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public UserInfo User { get; set; } = null!;
        public ShopInfo? Shop { get; set; }
    }

    public class UserInfo
    {
        public int UserId { get; set; }
        public int ClientID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }

    public class ShopInfo
    {
        public int ShopSettingId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? GSTIN { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public string? InvoicePrefix { get; set; }
    }

    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class AutoLoginResponse
    {
        public bool IsValid { get; set; }
        public LoginResponse? LoginData { get; set; }
        public string? Message { get; set; }
    }
    public class DeviceApproveRequest
    {
        [Required]
        public long RequestId { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty; // "APPROVE" ya "REJECT"

        public string? Remarks { get; set; }
    }

    public class PendingDeviceDto
    {
        public long RequestId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string ClientName { get; set; } = string.Empty;
    }
}