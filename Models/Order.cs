namespace OnlineShop.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal Total { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }
}