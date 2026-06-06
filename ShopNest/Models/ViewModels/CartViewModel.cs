namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/CartViewModel.cs
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();

        public decimal Subtotal => Items.Sum(i => i.TotalPrice);
        public decimal GST => Math.Round(Subtotal * 0.18m, 2);
        public decimal Total => Subtotal + GST;
        public int TotalItems => Items.Sum(i => i.Quantity);
    }
}
