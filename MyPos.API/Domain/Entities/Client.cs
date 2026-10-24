namespace MyPos.API.Domain.Entities
{
    public class Client
    {
        public int ClientID { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<ShopSetting> ShopSettings { get; set; } = new List<ShopSetting>();
    }
}