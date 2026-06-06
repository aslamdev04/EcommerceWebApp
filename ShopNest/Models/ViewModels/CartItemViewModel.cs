namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/CartItemViewModel.cs
    public class CartItemViewModel
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryIcon { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int Stock { get; set; }

        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
