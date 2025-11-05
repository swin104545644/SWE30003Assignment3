namespace OnlineShop.Models
{
    public class ConfirmOrderStockViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int OrderQuantity { get; set; }
        public int NewStock { get; set; }
    }
}