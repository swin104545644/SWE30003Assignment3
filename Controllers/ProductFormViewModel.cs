using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class ProductFormViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 1000)]
        public int Stock { get; set; } = 10;

        public string? ImageUrl { get; set; } // Current image

        public IFormFile? ImageFile { get; set; } // New upload
    }
}