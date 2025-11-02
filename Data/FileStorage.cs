using System.Text.Json;
using OnlineShop.Models;

namespace OnlineShop.Data
{
    public class FileStorage
    {
        private readonly string _appDataPath;
        private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

        public FileStorage(IWebHostEnvironment env)
        {
            _appDataPath = Path.Combine(env.ContentRootPath, "AppData");
            Directory.CreateDirectory(_appDataPath);
        }

        public List<User> LoadUsers()
        {
            var path = Path.Combine(_appDataPath, "users.json");
            if (!File.Exists(path)) return GetDefaultUsers();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<User>>(json) ?? GetDefaultUsers();
        }

        public void SaveUsers(List<User> users)
        {
            var json = JsonSerializer.Serialize(users, _options);
            File.WriteAllText(Path.Combine(_appDataPath, "users.json"), json);
        }

        public List<Product> LoadProducts()
        {
            var path = Path.Combine(_appDataPath, "products.json");
            if (!File.Exists(path)) return GetDefaultProducts();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Product>>(json) ?? GetDefaultProducts();
        }

        public void SaveProducts(List<Product> products)
        {
            var json = JsonSerializer.Serialize(products, _options);
            File.WriteAllText(Path.Combine(_appDataPath, "products.json"), json);
        }

		public Dictionary<int, List<CartItem>> LoadCart()
		{
			var path = Path.Combine(_appDataPath, "cart.json");
			if (!File.Exists(path)) return new();
			var json = File.ReadAllText(path);
			return JsonSerializer.Deserialize<Dictionary<int, List<CartItem>>>(json) ?? new();
		}

		public void SaveCart(Dictionary<int, List<CartItem>> cart)
		{
			var json = JsonSerializer.Serialize(cart, _options);
			File.WriteAllText(Path.Combine(_appDataPath, "cart.json"), json);
		}
		
		public List<Order> LoadOrders()
		{
			var path = Path.Combine(_appDataPath, "orders.json");
			if (!File.Exists(path)) return new List<Order>();
			var json = File.ReadAllText(path);
			return JsonSerializer.Deserialize<List<Order>>(json) ?? new List<Order>();
		}

		public void SaveOrders(List<Order> orders)
		{
			var json = JsonSerializer.Serialize(orders, _options);
			File.WriteAllText(Path.Combine(_appDataPath, "orders.json"), json);
		}

		public bool ReduceStock(int productId, int quantity)
		{
			var products = LoadProducts();
			var product = products.FirstOrDefault(p => p.Id == productId);
			if (product == null || product.Stock < quantity) return false;

			product.Stock -= quantity;
			SaveProducts(products);
			return true;
		}

        private List<User> GetDefaultUsers()
        {
            return new List<User>
            {
                new User
                {
                    Id = 1,
                    Name = "Admin",
                    Email = "admin@shop.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    IsAdmin = true
                }
            };
        }

        private List<Product> GetDefaultProducts()
        {
            return new List<Product>
            {
                new Product { Id = 1, Name = "Laptop", Description = "High-end gaming laptop", Price = 1299.99m, ImageUrl = "/images/products/noimage.png", Stock = 5 },
                new Product { Id = 2, Name = "Mouse", Description = "Wireless ergonomic mouse", Price = 29.99m, ImageUrl = "/images/products/noimage.png", Stock = 50 }
            };
        }
    }
}