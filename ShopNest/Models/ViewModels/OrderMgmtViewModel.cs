namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/OrderMgmtViewModel.cs
    public class OrderMgmtViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerInitials { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }

        public string FormattedDate => OrderDate.ToString("MMM dd, yyyy");

        public string StatusBadgeClass => Status switch
        {
            "Delivered" => "bdg-delivered",
            "Shipped" => "bdg-shipped",
            "Processing" => "bdg-processing",
            "Pending" => "bdg-pending",
            "Cancelled" => "bdg-cancelled",
            _ => "bdg-active"
        };

        public string AvatarColor => Status switch
        {
            "Delivered" => "background:#d1fae5;color:var(--emerald)",
            "Shipped" => "background:var(--blue-l);color:var(--blue)",
            "Processing" => "background:#fef3c7;color:var(--amber)",
            "Cancelled" => "background:#ffe4e6;color:var(--rose)",
            _ => "background:#ede9fe;color:var(--violet)"
        };
    }
}
