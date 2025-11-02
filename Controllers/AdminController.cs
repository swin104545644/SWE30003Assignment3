using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
		private readonly AppDbContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IWebHostEnvironment _environment;

		public AdminController(AppDbContext context, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

		private int CurrentUserId => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirst("UserId")!.Value);

        public IActionResult Index()
        {
            var products = _context.Products.ToList();
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

				_context.Products.Add(product);
				_context.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(model);
		}

		[HttpGet]
		public IActionResult Edit(int id)
		{
			var product = _context.Products.Find(id);
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
				var product = _context.Products.Find(id);
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

				_context.SaveChanges();
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
		public IActionResult Delete(int id)
		{
			var product = _context.Products.Find(id);
			if (product != null)
			{
				_context.Products.Remove(product);
				_context.SaveChanges();
			}
			return RedirectToAction("Index");
		}

		public IActionResult Users()
		{
			var users = _context.Users.ToList();
			return View(users);
		}

		[HttpGet]
		public IActionResult CreateUser() => View();

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult CreateUser(User user, bool makeAdmin = false)
		{
			if (_context.Users.Any(u => u.Email == user.Email))
			{
				ModelState.AddModelError("Email", "Email already exists");
				return View(user);
			}

			if (ModelState.IsValid)
			{
				var currentUser = _context.Users.Find(CurrentUserId);
				if (!currentUser!.IsAdmin && makeAdmin)
				{
					ModelState.AddModelError("", "You are not authorized to create admin accounts.");
					return View(user);
				}

				user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
				user.IsAdmin = makeAdmin && currentUser.IsAdmin;
				_context.Users.Add(user);
				_context.SaveChanges();
				return RedirectToAction("Users");
			}
			return View(user);
		}

		[HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleAdmin(int id)
        {
            var user = _context.Users.Find(id);
            var currentUser = _context.Users.Find(CurrentUserId);

            if (user == null || !currentUser!.IsAdmin) return Forbid();

            if (user.IsAdmin && _context.Users.Count(u => u.IsAdmin) <= 1)
            {
                TempData["Error"] = "Cannot remove the last admin.";
                return RedirectToAction("Users");
            }

            user.IsAdmin = !user.IsAdmin;
            _context.SaveChanges();
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null || user.Id == CurrentUserId) return Forbid();

            if (user.IsAdmin && _context.Users.Count(u => u.IsAdmin) <= 1)
            {
                TempData["Error"] = "Cannot delete the last admin.";
                return RedirectToAction("Users");
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return RedirectToAction("Users");
        }

		[HttpGet]
		public IActionResult ForgotPassword() => View();

		[HttpPost]
		public async Task<IActionResult> ForgotPassword(string email)
		{
			var user = _context.Users.FirstOrDefault(u => u.Email == email);
			if (user == null)
			{

				return RedirectToAction("ForgotPasswordConfirmation");
			}

	
			var token = Guid.NewGuid().ToString();
			user.PasswordResetToken = token;
			user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);
			_context.SaveChanges();

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

			var user = _context.Users.FirstOrDefault(u => u.Email == email);
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

			var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
			if (user == null || user.PasswordResetToken != model.Token || 
				user.PasswordResetTokenExpires < DateTime.UtcNow)
			{
				return RedirectToAction("ResetPasswordExpired");
			}

			user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
			user.PasswordResetToken = null;
			user.PasswordResetTokenExpires = null;
			_context.SaveChanges();

			return RedirectToAction("ResetPasswordSuccess");
		}

		public IActionResult ResetPasswordExpired() => View();
		public IActionResult ResetPasswordSuccess() => View();
    }
}