using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var enriched = GetEnrichedCart();
            ViewBag.Total = enriched.Sum(x => x.LineTotal);
            return View(enriched);
        }

        [HttpPost]
        public IActionResult Update(int productId, int quantity)
        {
            var product = _context.Products.Find(productId);
            if (product == null || quantity > product.Stock || quantity <= 0)
            {
                TempData["Error"] = product == null
                    ? "Product no longer exists."
                    : $"Only {product.Stock} available.";
                return RedirectToAction("Index");
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0) cart.Remove(item);
                else item.Quantity = quantity;
            }
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(i => i.ProductId == productId);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var enriched = GetEnrichedCart();
            var outOfStock = enriched.Where(x => !x.IsInStock).ToList();
            if (outOfStock.Any())
            {
                TempData["CartWarning"] = "Some items are out of stock. Please update your cart.";
                return RedirectToAction("Index");
            }
            return View(enriched);
        }

        [HttpPost]
        public IActionResult ConfirmCheckout()
        {
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("PaymentSuccess");
        }

        public IActionResult PaymentSuccess() => View();

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

        private List<EnrichedCartItem> GetEnrichedCart()
        {
            var cart = GetCart();
            var enriched = new List<EnrichedCartItem>();
            foreach (var item in cart)
            {
                var product = _context.Products.Find(item.ProductId);
                if (product != null)
                {
                    enriched.Add(new EnrichedCartItem
                    {
                        CartItem = item,
                        Product = product
                    });
                }
            }
            return enriched;
        }
    }
}