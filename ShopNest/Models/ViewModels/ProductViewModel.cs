using System.ComponentModel.DataAnnotations;

namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/ProductViewModel.cs
    public class ProductViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(1, 999999, ErrorMessage = "Enter valid price")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Stock is required")]
        [Range(0, 99999, ErrorMessage = "Enter valid stock")]
        public int? Stock { get; set; }

        //public string SKU { get; set; }

        public string? Description { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
