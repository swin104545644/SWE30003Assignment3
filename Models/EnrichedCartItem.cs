using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
	public class EnrichedCartItem
	{
		public CartItem CartItem { get; set; } = null!;
		public Product Product { get; set; } = null!;
		public int AvailableStock => Product.Stock;
		public bool IsInStock => Product.Stock >= CartItem.Quantity;
		public decimal LineTotal => Product.Price * CartItem.Quantity;
	}
}