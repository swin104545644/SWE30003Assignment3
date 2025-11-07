namespace OnlineShop.Models
{
    public class DeliveryMethod
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool isActive { get; set; } = true;
        public decimal BasePrice { get; set; } = 0m;
        public decimal PerKmPrice { get; set; } = 0.10m;
        public decimal FreeOverAmount { get; set; } = 75m;
        public int MaxDistanceKm { get; set; } = 500;
    }
}