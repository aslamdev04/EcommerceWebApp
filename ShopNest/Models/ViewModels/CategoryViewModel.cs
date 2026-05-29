using System.ComponentModel.DataAnnotations;

namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/CategoryViewModel.cs
    public class CategoryViewModel
    {
        [Required(ErrorMessage = "Icon is required")]
        public string Icon { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        public string CategoryName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
