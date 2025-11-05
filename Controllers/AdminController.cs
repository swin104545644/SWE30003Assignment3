using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
		private readonly FileStorage _context;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IWebHostEnvironment _environment;

		public AdminController(FileStorage context, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

		private int CurrentUserId => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst("UserId")!.Value);

        public IActionResult Index()
        {
            var products = _context.LoadProducts();
            return View(products);
        }

		[HttpGet]
		public IActionResult Create()
		{
			return View(new ProductFormViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ProductFormViewModel model)
		{
			
			if (ModelState.IsValid)
			{

				var products = _context.LoadProducts();
				var product = new Product
				{
					Name = model.Name,
					Description = model.Description,
					Price = model.Price,
					Stock = model.Stock,
					ImageUrl = "/images/products/noimage.png"
				};

				if (model.ImageFile != null)
				{
					product.ImageUrl = await UploadImage(model.ImageFile);
				}

				products.Add(product);
				_context.SaveProducts(products);
				return RedirectToAction("Index");
			}
			return View(model);
		}

		[HttpGet]
		public IActionResult Edit(int id)
		{

			var products = _context.LoadProducts();
			var product = products.FirstOrDefault(p => p.Id == id);
			if (product == null) return NotFound();

			var model = new ProductFormViewModel
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				Stock = product.Stock,
				ImageUrl = product.ImageUrl
			};

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
		{
			if (id != model.Id) return NotFound();

			if (ModelState.IsValid)
			{

				var products = _context.LoadProducts();
				var product = products.FirstOrDefault(p => p.Id == id);
				if (product == null) return NotFound();

				if (model.ImageFile != null)
				{
					DeleteImage(product.ImageUrl);
					product.ImageUrl = await UploadImage(model.ImageFile);
				}

				product.Name = model.Name;
				product.Description = model.Description;
				product.Price = model.Price;
				product.Stock = model.Stock;

				_context.SaveProducts(products);
				return RedirectToAction("Index");
			}
			return View(model);
		}

		private async Task<string> UploadImage(IFormFile imageFile)
		{
			
			if (imageFile == null || imageFile.Length == 0)
				return "/images/products/noimage.png";

			var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
			var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
			if (!allowedExtensions.Contains(extension))
				throw new InvalidOperationException("Invalid file type. Only JPG, PNG, GIF allowed.");

			if (imageFile.Length > 5 * 1024 * 1024)
				throw new InvalidOperationException("File too large. Max 5MB.");

			
			var filename = $"{Guid.NewGuid()}{extension}";
			var path = Path.Combine(_environment.WebRootPath, "images/products", filename);

			using (var stream = new FileStream(path, FileMode.Create))
			{
				await imageFile.CopyToAsync(stream);
			}

			return $"/images/products/{filename}";
		}
		
		private void DeleteImage(string imagePath)
        {
            if (imagePath.StartsWith("/images/products/") && imagePath != "/images/products/noimage.png")
            {
                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
        }

		[HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var products = _context.LoadProducts();
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            DeleteImage(product.ImageUrl);
            products.Remove(product);
            _context.SaveProducts(products);
            return RedirectToAction("Index");
        }

		public IActionResult Users()
        {
            var users = _context.LoadUsers();
            return View(users);
        }

		[HttpGet]
		public IActionResult CreateUser() => View();

		[HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(User user, bool makeAdmin = false)
        {
            var users = _context.LoadUsers();
            if (users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(user);
            }

            if (ModelState.IsValid)
            {
                var currentUser = users.FirstOrDefault(u => u.Id == CurrentUserId);
                if (!currentUser!.IsAdmin && makeAdmin)
                {
                    ModelState.AddModelError("", "Not authorized");
                    return View(user);
                }

                user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                user.IsAdmin = makeAdmin && currentUser.IsAdmin;
                users.Add(user);
                _context.SaveUsers(users);
                return RedirectToAction("Users");
            }
            return View(user);
        }

		[HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleAdmin(int id)
        {
            var users = _context.LoadUsers();
            var user = users.FirstOrDefault(u => u.Id == id);
            var currentUser = users.FirstOrDefault(u => u.Id == CurrentUserId);

            if (user == null || !currentUser!.IsAdmin) return Forbid();

            if (user.IsAdmin && users.Count(u => u.IsAdmin) <= 1)
            {
                TempData["Error"] = "Cannot remove the last admin.";
                return RedirectToAction("Users");
            }

            user.IsAdmin = !user.IsAdmin;
            _context.SaveUsers(users);
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            var users = _context.LoadUsers();
            var user = users.FirstOrDefault(u => u.Id == id);
            if (user == null || user.Id == CurrentUserId) return Forbid();

            if (user.IsAdmin && users.Count(u => u.IsAdmin) <= 1)
            {
                TempData["Error"] = "Cannot delete the last admin.";
                return RedirectToAction("Users");
            }

            users.Remove(user);
            _context.SaveUsers(users);
            return RedirectToAction("Users");
        }

		public IActionResult Stats()
		{
			var context = HttpContext.RequestServices.GetRequiredService<FileStorage>();
			var orders = context.LoadOrders();

			var stats = new SalesStatsViewModel
			{
				TotalOrders = orders.Count,
				TotalRevenue = orders.Sum(o => o.Total),
				PeriodStats = new Dictionary<string, PeriodStats>
				{
					["Today"] = GetPeriodStats(orders, TimeSpan.FromDays(1)),
					["Last 7 Days"] = GetPeriodStats(orders, TimeSpan.FromDays(7)),
					["Last 30 Days"] = GetPeriodStats(orders, TimeSpan.FromDays(30)),
					["All Time"] = GetPeriodStats(orders, TimeSpan.FromDays(365 * 10))
				},
				TopProducts = orders
					.SelectMany(o => o.Items)
					.GroupBy(i => i.ProductName)
					.Select(g => new TopProduct { Name = g.Key, Quantity = g.Sum(i => i.Quantity), Revenue = g.Sum(i => i.Price * i.Quantity) })
					.OrderByDescending(p => p.Revenue)
					.Take(5)
					.ToList()
			};

			return View(stats);
		}

		private PeriodStats GetPeriodStats(List<Order> orders, TimeSpan period)
		{
			var cutoff = DateTime.UtcNow.Add(-period);
			var periodOrders = orders.Where(o => o.OrderDate >= cutoff).ToList();
			return new PeriodStats
			{
				Orders = periodOrders.Count,
				Revenue = periodOrders.Sum(o => o.Total)
			};
		}

		[HttpGet]
		public IActionResult ForgotPassword() => View();

		[HttpPost]
		public async Task<IActionResult> ForgotPassword(string email)
		{

			var users = _context.LoadUsers();
			var user = users.FirstOrDefault(u => u.Email == email);

			if (user == null)
			{

				return RedirectToAction("ForgotPasswordConfirmation");
			}

	
			var token = Guid.NewGuid().ToString();
			user.PasswordResetToken = token;
			user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);
			_context.SaveUsers(users);

			var resetLink = Url.Action("ResetPassword", "Account",
				new { email = user.Email, token }, Request.Scheme);

			TempData["ResetLink"] = resetLink; 
			return RedirectToAction("ForgotPasswordConfirmation");
		}

		public IActionResult ForgotPasswordConfirmation()
		{
			ViewBag.Link = TempData["ResetLink"];
			return View();
		}

		[HttpGet]
		public IActionResult ResetPassword(string email, string token)
		{
			if (email == null || token == null) return RedirectToAction("Login");

			var users = _context.LoadUsers();
			var user = users.FirstOrDefault(u => u.Email == email);
			if (user == null || user.PasswordResetToken != token || 
				user.PasswordResetTokenExpires < DateTime.UtcNow)
			{
				return RedirectToAction("ResetPasswordExpired");
			}

			var model = new ResetPasswordViewModel { Email = email, Token = token };
			return View(model);
		}

		[HttpPost]
		public IActionResult ResetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid) return View(model);

			var users = _context.LoadUsers();
			var user = users.FirstOrDefault(u => u.Email == model.Email);

			if (user == null || user.PasswordResetToken != model.Token ||
				user.PasswordResetTokenExpires < DateTime.UtcNow)
			{
				return RedirectToAction("ResetPasswordExpired");
			}

			user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
			user.PasswordResetToken = null;
			user.PasswordResetTokenExpires = null;
			_context.SaveUsers(users);

			return RedirectToAction("ResetPasswordSuccess");
		}
		
		[HttpGet]
		public IActionResult OrderStock(int id)
		{
			var products = _context.LoadProducts();
			var product = products.FirstOrDefault(p => p.Id == id);
			if (product == null) return NotFound();

			var model = new OrderStockViewModel
			{
				ProductId = product.Id,
				ProductName = product.Name,
				CurrentStock = product.Stock
			};
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult OrderStock(OrderStockViewModel model)
		{
			if (!ModelState.IsValid) return View(model);

			return RedirectToAction("ConfirmOrderStock", new { id = model.ProductId, quantity = model.Quantity });
		}

		[HttpGet]
		public IActionResult ConfirmOrderStock(int id, int quantity)
		{
			var products = _context.LoadProducts();
			var product = products.FirstOrDefault(p => p.Id == id);
			if (product == null) return NotFound();

			var model = new ConfirmOrderStockViewModel
			{
				ProductId = product.Id,
				ProductName = product.Name,
				CurrentStock = product.Stock,
				OrderQuantity = quantity,
				NewStock = product.Stock + quantity
			};
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ConfirmOrderStock(ConfirmOrderStockViewModel model)
		{
			var products = _context.LoadProducts();
			var product = products.FirstOrDefault(p => p.Id == model.ProductId);
			if (product == null) return NotFound();

			product.Stock += model.OrderQuantity;
			_context.SaveProducts(products);

			TempData["Success"] = $"Ordered {model.OrderQuantity} × {product.Name}. New stock: {product.Stock}";
			return RedirectToAction("Index");
		}

		public IActionResult ResetPasswordExpired() => View();
		public IActionResult ResetPasswordSuccess() => View();
    }
}