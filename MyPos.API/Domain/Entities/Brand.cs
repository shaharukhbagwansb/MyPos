namespace MyPos.API.Domain.Entities
{
    public class Brand
    {
        public int BrandId { get; set; }
        public int ClientID { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //public Client Client { get; set; } = null!;
    }
}