namespace OnlineShop.Models
{
    public class SalesStatsViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public Dictionary<string, PeriodStats> PeriodStats { get; set; } = new();
        public List<TopProduct> TopProducts { get; set; } = new();
    }

    public class PeriodStats
    {
        public int Orders { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopProduct
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }
}