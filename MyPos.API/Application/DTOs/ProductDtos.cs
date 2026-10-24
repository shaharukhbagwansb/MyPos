using System.ComponentModel.DataAnnotations;

namespace MyPos.API.Application.DTOs
{
    public class CreateCategoryRequest
    {
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CreateBrandRequest
    {
        [Required]
        [MaxLength(100)]
        public string BrandName { get; set; } = string.Empty;
    }

    public class CreateUnitRequest
    {
        [Required]
        public string UnitName { get; set; } = string.Empty;
        [Required]
        public string ShortName { get; set; } = string.Empty;
    }

    public class CreateProductRequest
    {
        [Required]
        public string ProductName { get; set; } = string.Empty;
        public string? ProductCode { get; set; }
        public string? Barcode { get; set; }
        public string? ModelNo { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        [Required]
        public int UnitId { get; set; }
        public decimal PurchaseRate { get; set; }
        public decimal SaleRate { get; set; }
        public decimal MinimumStock { get; set; } = 0;
        public decimal TaxPercent { get; set; } = 0;
    }
}