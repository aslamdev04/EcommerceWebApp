using System.ComponentModel.DataAnnotations;

namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/ProfileViewModel.cs
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; }

        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Pincode { get; set; }

        // Display only
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }

        public string Initials
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) return "U";
                var parts = Name.Split(' ');
                if (parts.Length >= 2)
                    return $"{parts[0][0]}{parts[1][0]}".ToUpper();
                return Name.Substring(0, 1).ToUpper();
            }
        }
    }
}
