using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;
using OnlineShop.Models;
using System.Security.Claims;

namespace OnlineShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly FileStorage _context;

        public AccountController(FileStorage context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Signup() => View();

		[HttpPost]
		public IActionResult Signup(User user)
		{
			var users = _context.LoadUsers();
			if (users.Any(u => u.Email == user.Email))
			{
				ModelState.AddModelError("Email", "Email exists");
				return View(user);
			}

			if (ModelState.IsValid)
			{
				user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
				user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
				user.IsAdmin = !users.Any(); // First user = admin
				users.Add(user);
				_context.SaveUsers(users);
				return RedirectToAction("Login");
			}
			return View(user);
		}

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
		public async Task<IActionResult> Login(string email, string password)
		{
			var users = _context.LoadUsers();
			var user = users.FirstOrDefault(u => u.Email == email);
			if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
			{
				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, user.Name),
					new Claim(ClaimTypes.Email, user.Email),
					new Claim("UserId", user.Id.ToString())
				};
				if (user.IsAdmin) claims.Add(new Claim(ClaimTypes.Role, "Admin"));

				var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
				await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
				return RedirectToAction("Index", "Products");
			}
			ModelState.AddModelError("", "Invalid login");
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

			var users = _context.LoadUsers();
			var user = users.FirstOrDefault(u => u.Email == email);

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

			var users = _context.LoadUsers();
			var user = users.FirstOrDefault(u => u.Email == model.Email);

			if (user == null ||
				user.PasswordResetToken != model.Token ||
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
		public IActionResult ResetPasswordSuccess()
		{
			return View();
		}
    }
}