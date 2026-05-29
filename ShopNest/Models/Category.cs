namespace ShopNest.Models
{
    // Models/Category.cs
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Icon { get; set; }
        public bool IsActive { get; set; } = true;  // ← Add karo
        public List<Product> Products { get; set; }
    }
}
