namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/ProductBrowseViewModel.cs
    public class ProductBrowseViewModel
    {
        public int? ProductId { get; set; }
        public string? Name { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryIcon { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public string? ImageUrl { get; set; }

        public bool IsOutOfStock => Stock == 0;
    }
}
