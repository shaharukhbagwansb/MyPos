namespace MyPos.API.Domain.Entities
{
    public class Unit
    {
        public int UnitId { get; set; }
        public int ClientID { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        //public Client Client { get; set; } = null!;
    }
}