namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/MyOrderViewModel.cs
    public class MyOrderViewModel
    {
        public int OrderId { get; set; }
        public string FirstProductName { get; set; }
        public string FirstProductIcon { get; set; }
        public int TotalItems { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        // Display helpers
        public string ProductSummary => TotalItems > 1
            ? $"{FirstProductIcon} {FirstProductName} + {TotalItems - 1} more"
            : $"{FirstProductIcon} {FirstProductName}";

        public string StatusBadgeClass => Status switch
        {
            "Delivered" => "bdg-delivered",
            "Shipped" => "bdg-shipped",
            "Processing" => "bdg-processing",
            "Pending" => "bdg-pending",
            "Cancelled" => "bdg-oos",
            _ => "bdg-active"
        };

        public string FormattedDate => OrderDate.ToString("MMM dd, yyyy");
    }
}
