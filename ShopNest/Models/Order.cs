namespace ShopNest.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ?Status { get; set; }
        public string ?Address { get; set; }
        public User ?User { get; set; }
        public List<OrderItem> ?OrderItems { get; set; }
    }
}
