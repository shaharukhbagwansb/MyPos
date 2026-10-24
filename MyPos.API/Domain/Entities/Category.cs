namespace MyPos.API.Domain.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public int ClientID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Client Client { get; set; } = null!;
    }
}