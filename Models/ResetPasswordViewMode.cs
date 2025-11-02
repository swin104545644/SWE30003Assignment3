// Models/ResetPasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class ResetPasswordViewModel
    {
        [Required] public string Email { get; set; } = string.Empty;
        [Required] public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}