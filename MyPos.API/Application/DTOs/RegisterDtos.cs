using System.ComponentModel.DataAnnotations;

namespace MyPos.API.Application.DTOs
{
    // SuperAdmin → Naya Shop + Shop Owner banaye
    public class RegisterShopRequest
    {
        [Required]
        public string ShopName { get; set; } = string.Empty;

        public string? City { get; set; }
        public string? State { get; set; }
        public string? Mobile { get; set; }
        public string? GSTIN { get; set; }

        // Shop Owner details
        [Required]
        public string OwnerUserName { get; set; } = string.Empty;

        [Required]
        public string OwnerPassword { get; set; } = string.Empty;

        [Required]
        public string OwnerFullName { get; set; } = string.Empty;

        public string? OwnerMobile { get; set; }
    }

    // Admin → Staff banaye
    public class RegisterStaffRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? Mobile { get; set; }

        [Required]
        public string RoleName { get; set; } = string.Empty; // SALES, PURCHASE, ACCOUNT
    }
}