using System.ComponentModel.DataAnnotations;

namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/CheckoutViewModel.cs
    public class CheckoutViewModel
    {
        // Cart Items
        public List<CartItemViewModel> Items { get; set; } = new();

        // Address
        [Required(ErrorMessage = "Address is required")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string? City { get; set; }

        [Required(ErrorMessage = "Pincode is required")]
        public string? Pincode { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        public string? Phone { get; set; }

        // Delivery Option
        public string DeliveryOption { get; set; } = "Standard";

        // Calculated
        public decimal Subtotal => Items.Sum(i => i.TotalPrice);
        public decimal GST => Math.Round(Subtotal * 0.18m, 2);
        public decimal DeliveryCharge => DeliveryOption switch
        {
            "Express" => 149,
            "SameDay" => 299,
            _ => 0
        };
        public decimal Total => Subtotal + GST + DeliveryCharge;
        public int TotalItems => Items.Sum(i => i.Quantity);
    }
}
