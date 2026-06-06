namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/ReportsViewModel.cs
    public class ReportsViewModel
    {
        // Stats Cards
        public int NewUsersThisMonth { get; set; }
        public int OrdersThisMonth { get; set; }
        public string RevenueThisMonthK { get; set; }
        public double ReturnRate { get; set; }

        // Top Categories
        public List<CategoryRevenueViewModel> CategoryRevenue { get; set; } = new();

        // Top Products
        public List<TopProductViewModel> TopProducts { get; set; } = new();
    }

    public class CategoryRevenueViewModel
    {
        public string CategoryName { get; set; }
        public string Icon { get; set; }
        public string RevenueK { get; set; }
        public int Percentage { get; set; }
    }

    public class TopProductViewModel
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public int TotalSales { get; set; }
        public string RevenueK { get; set; }
    }
}
