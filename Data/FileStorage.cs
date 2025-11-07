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
            if (!File.Exists(path))
            {
                var defaults = GetDefaultUsers();
                SaveUsers(defaults);
                return defaults;
            }

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
            if (!File.Exists(path))
            {
                var defaults = GetDefaultProducts();
                SaveProducts(defaults);
                return defaults;
            }

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
            if (!File.Exists(path))
            {
                var defaults = new Dictionary<int, List<CartItem>>();
                SaveCart(defaults);  // ← Save empty cart on first load
                return defaults;
            }

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
            if (!File.Exists(path))
            {
                var defaults = new List<Order>();
                SaveOrders(defaults);  // ← Save empty list on first load
                return defaults;
            }

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Order>>(json) ?? new List<Order>();
        }

        public void SaveOrders(List<Order> orders)
        {
            var json = JsonSerializer.Serialize(orders, _options);
            File.WriteAllText(Path.Combine(_appDataPath, "orders.json"), json);
        }

        public List<Delivery> LoadDeliveries()
        {
            var path = Path.Combine(_appDataPath, "deliveries.json");
            if (!File.Exists(path))
            {
                var defaults = new List<Delivery>();
                SaveDeliveries(defaults);
                return defaults;
            }

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Delivery>>(json) ?? new List<Delivery>();
        }

        public void SaveDeliveries(List<Delivery> deliveries)
        {
            var json = JsonSerializer.Serialize(deliveries, _options);
            File.WriteAllText(Path.Combine(_appDataPath, "deliveries.json"), json);
        }

        public List<DeliveryAddress> LoadDeliveryAddresses()
        {
            var path = Path.Combine(_appDataPath, "delivery_addresses.json");
            if (!File.Exists(path))
            {
                var defaults = new List<DeliveryAddress>();
                SaveDeliveryAddresses(defaults);
                return defaults;
            }

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<DeliveryAddress>>(json) ?? new List<DeliveryAddress>();
        }

        public void SaveDeliveryAddresses(List<DeliveryAddress> addresses)
        {
            var json = JsonSerializer.Serialize(addresses, _options);
            File.WriteAllText(Path.Combine(_appDataPath, "delivery_addresses.json"), json);
        }

        public List<DeliveryMethod> LoadDeliveryMethods()
        {
            var path = Path.Combine(_appDataPath, "delivery_methods.json");
            if (!File.Exists(path))
            {
                // Provide some example defaults if the file doesn’t exist yet
                var defaults = new List<DeliveryMethod>
                {
                    new DeliveryMethod
                    {
                        Id = 1,
                        Name = "Standard Delivery",
                        isActive = true,
                        BasePrice = 5.00m,
                        PerKmPrice = 0.10m,
                        FreeOverAmount = 75m,
                        MaxDistanceKm = 500
                    },
                    new DeliveryMethod
                    {
                        Id = 2,
                        Name = "Express Delivery",
                        isActive = true,
                        BasePrice = 10.00m,
                        PerKmPrice = 0.20m,
                        FreeOverAmount = 150m,
                        MaxDistanceKm = 500
                    }
                };

                SaveDeliveryMethods(defaults);
                return defaults;
            }

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<DeliveryMethod>>(json) ?? new List<DeliveryMethod>();
        }

        public void SaveDeliveryMethods(List<DeliveryMethod> methods)
        {
            var json = JsonSerializer.Serialize(methods, _options);
            File.WriteAllText(Path.Combine(_appDataPath, "delivery_methods.json"), json);
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
                    Email = "admin@admin.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                    IsAdmin = true
                }
            };
        }

        private List<Product> GetDefaultProducts()
        {
            return new List<Product>
            {
                new Product { Id = 1, Name = "Apple", Description = "Definitely not poisonous...", Price = 1299.99m, ImageUrl = "/images/products/sample1.png", Stock = 5 },
                new Product { Id = 2, Name = "Toy Transformer", Description = "Definitely not a Decepticon", Price = 29.99m, ImageUrl = "/images/products/sample2.png", Stock = 50 }
            };
        }
    }
}