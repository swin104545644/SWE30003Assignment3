using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

		public IActionResult Index(string search)
		{
			var products = string.IsNullOrWhiteSpace(search)
				? _context.Products.ToList()
				: _context.Products
					.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
					.ToList();

			ViewBag.Search = search;
			return View(products);
		}

		public IActionResult Details(int id)
		{
			var product = _context.Products.Find(id);
			if (product == null) return NotFound();
			return View(product);
		}

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            var cart = GetCart();
            var existing = cart.FirstOrDefault(i => i.ProductId == productId);
            int newQty = quantity + (existing?.Quantity ?? 0);

            if (newQty > product.Stock)
            {
                TempData["Error"] = $"Only {product.Stock} unit(s) of {product.Name} available.";
                return RedirectToAction("Details", new { id = productId });
            }

            if (existing == null)
                cart.Add(new CartItem { ProductId = productId, Quantity = quantity });
            else
                existing.Quantity = newQty;

            SaveCart(cart);
            return RedirectToAction("Index", "Cart");
        }

        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            return cartJson == null ? new List<CartItem>() :
                System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cartJson)!;
        }

        private void SaveCart(List<CartItem> cart)
        {
            var cartJson = System.Text.Json.JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }
    }
}