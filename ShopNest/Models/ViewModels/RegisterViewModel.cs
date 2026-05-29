using System.ComponentModel.DataAnnotations;

namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/RegisterViewModel.cs
    public class RegisterViewModel
    {
        [Required]
        public string Name { get; set; }


        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match!")]
        public string ConfirmPassword { get; set; }
    }
}
