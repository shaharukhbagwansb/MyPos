namespace MyPos.API.Domain.Entities
{
    public class DeviceApprovalRequest
    {
        public long RequestId { get; set; }
        public int ClientID { get; set; }
        public int UserId { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? Remarks { get; set; }

        public User User { get; set; } = null!;
        public Client Client { get; set; } = null!;
    }
}