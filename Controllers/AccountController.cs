using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;
using OnlineShop.Models;
using System.Security.Claims;
using BCrypt.Net;

namespace OnlineShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Signup() => View();

		[HttpPost]
		public IActionResult Signup(User user)
		{
			if (_context.Users.Any(u => u.Email == user.Email))
			{
				ModelState.AddModelError("Email", "Email already exists");
				return View(user);
			}

			user.IsAdmin = false;
			user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
			_context.Users.Add(user);
			_context.SaveChanges();

			return RedirectToAction("Login");
		}

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
			{
				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, user.Name),
					new Claim(ClaimTypes.Email, user.Email),
					new Claim("UserId", user.Id.ToString())
				};

				if (user.IsAdmin)
					claims.Add(new Claim(ClaimTypes.Role, "Admin"));

				var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
				var principal = new ClaimsPrincipal(identity);
				await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

				return RedirectToAction("Index", "Products");
			}

            ModelState.AddModelError("", "Invalid login attempt");
            return View();
        }

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("Index", "Products");
		}
		
		[HttpGet]
		public IActionResult ForgotPassword() => View();

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ForgotPassword(string email)
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

			var resetLink = Url.Action(
				"ResetPassword",
				"Account",
				new { email = user.Email, token },
				Request.Scheme
			);

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
			if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
				return RedirectToAction("Login");

			var user = _context.Users.FirstOrDefault(u => u.Email == email);
			if (user == null ||
				user.PasswordResetToken != token ||
				user.PasswordResetTokenExpires < DateTime.UtcNow)
			{
				return RedirectToAction("ResetPasswordExpired");
			}

			var model = new ResetPasswordViewModel { Email = email, Token = token };
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ResetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid) return View(model);

			var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
			if (user == null ||
				user.PasswordResetToken != model.Token ||
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
		
		[HttpGet]
		public IActionResult ResetPasswordSuccess()
		{
			return View();
		}
    }
}