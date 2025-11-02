using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 10000, ErrorMessage = "Price must be > 0")]
        public decimal Price { get; set; }

        [Url(ErrorMessage = "Invalid URL")]
        public string ImageUrl { get; set; } = "/images/products/noimage.png"; // Fallback

        [Required]
        [Range(0, 1000, ErrorMessage = "Stock must be >= 0")]
        public int Stock { get; set; } = 10;
    }
}