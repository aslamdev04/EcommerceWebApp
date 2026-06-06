namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/RecentOrderViewModel.cs
    public class RecentOrderViewModel
    {
        public int OrderId { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        public string StatusBadgeClass => Status switch
        {
            "Delivered" => "bdg-delivered",
            "Shipped" => "bdg-shipped",
            "Processing" => "bdg-processing",
            "Pending" => "bdg-pending",
            _ => "bdg-active"
        };
    }
}
