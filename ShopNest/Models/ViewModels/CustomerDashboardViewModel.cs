namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/CustomerDashboardViewModel.cs
    public class CustomerDashboardViewModel
    {
        // User Info
        public string UserName { get; set; }

        // Order Stats
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int InTransitOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal TotalSpent { get; set; }

        // Recent Orders
        public List<RecentOrderViewModel> RecentOrders { get; set; }
    }
}
