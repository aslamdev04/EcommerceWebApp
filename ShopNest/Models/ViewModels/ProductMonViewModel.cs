namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/ProductMonViewModel.cs
    public class ProductMonViewModel
    {
        public int? ProductId { get; set; }
        public string? Name { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryIcon { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public string? ImageUrl { get; set; }

        // Stock Status
        public string? StockStatus => Stock == 0 ? "Out of Stock"
                                   : Stock <= 10 ? "Low Stock"
                                   : "In Stock";

        public string? StockBadgeClass => Stock == 0 ? "bdg-oos"
                                       : Stock <= 10 ? "bdg-low"
                                       : "bdg-instock";

        public string? StockColor => Stock == 0 ? "var(--rose)"
                                  : Stock <= 10 ? "var(--amber)"
                                  : "var(--emerald)";
    }
}
