using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class OrderStockViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Quantity must be 1–1000")]
        public int Quantity { get; set; }
    }
}