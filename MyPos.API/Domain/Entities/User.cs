namespace MyPos.API.Domain.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public int ClientID { get; set; }
        public int RoleId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Auto Login ke liye
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        public Client Client { get; set; } = null!;
        public Role Role { get; set; } = null!;
        // Device Binding
        public string? RegisteredDeviceId { get; set; }
        public string? RegisteredPlatform { get; set; }
        public DateTime? DeviceRegisteredAt { get; set; }
        public string? Email { get; set; }
    }
}