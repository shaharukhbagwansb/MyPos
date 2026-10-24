namespace MyPos.API.Domain.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public int ClientID { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int UnitId { get; set; }
        public string? ProductCode { get; set; }
        public string? Barcode { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ModelNo { get; set; }
        public decimal PurchaseRate { get; set; }
        public decimal SaleRate { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal TaxPercent { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Client Client { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public Brand? Brand { get; set; }
        public Unit Unit { get; set; } = null!;
    }
}