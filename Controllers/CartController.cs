using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    public class CartController : Controller
    {
        private readonly FileStorage _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartController(FileStorage storage, IHttpContextAccessor httpContextAccessor)
        {
            _context = storage;
            _httpContextAccessor = httpContextAccessor;
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

            var products = _context.LoadProducts();
            var product = products.FirstOrDefault(u => u.Id == productId);
            
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
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmCheckout()
        {
            var enriched = GetEnrichedCart();
            if (!enriched.All(x => x.IsInStock))
            {
                TempData["CartWarning"] = "Some items are out of stock.";
                return RedirectToAction("Index");
            }
            var context = HttpContext.RequestServices.GetRequiredService<FileStorage>();
            var orders = context.LoadOrders();
            var userId = CurrentUserId;

            var order = new Order
            {
                Id = orders.Any() ? orders.Max(o => o.Id) + 1 : 1,
                UserId = userId,
                Total = enriched.Sum(x => x.LineTotal)
            };

            foreach (var item in enriched)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.CartItem.ProductId,
                    ProductName = item.Product.Name,
                    Price = item.Product.Price,
                    Quantity = item.CartItem.Quantity
                });

                if (!context.ReduceStock(item.CartItem.ProductId, item.CartItem.Quantity))
                {
                    TempData["Error"] = "Stock changed during checkout.";
                    return RedirectToAction("Index");
                }
            }

            orders.Add(order);
            context.SaveOrders(orders);

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("PaymentSuccess");
        }

        private int CurrentUserId => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst("UserId")!.Value);

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
            var products = _context.LoadProducts();
            foreach (var item in cart)
            {
                var product = products.FirstOrDefault(u => u.Id == item.ProductId);
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