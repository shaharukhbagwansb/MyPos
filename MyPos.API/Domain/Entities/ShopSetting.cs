namespace MyPos.API.Domain.Entities
{
    public class ShopSetting
    {
        public int ShopSettingId { get; set; }
        public int ClientID { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? GSTIN { get; set; }
        public string? InvoicePrefix { get; set; }
        public int NextInvoiceNumber { get; set; } = 1;
        public string CurrencyCode { get; set; } = "INR";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Client Client { get; set; } = null!;
    }
}