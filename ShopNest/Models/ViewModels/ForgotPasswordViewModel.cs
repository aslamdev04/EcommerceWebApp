using System.ComponentModel.DataAnnotations;

namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/ForgotPasswordViewModel.cs
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; }
    }

    // Models/ViewModels/ResetPasswordViewModel.cs
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Minimum 6 characters")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match!")]
        public string ConfirmPassword { get; set; }
    }
}
