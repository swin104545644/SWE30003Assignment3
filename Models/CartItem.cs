using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
	public class CartItem
	{
		public int ProductId { get; set; }
		public int Quantity { get; set; }
	}
}