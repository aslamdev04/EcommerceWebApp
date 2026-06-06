namespace ShopNest.Models
{
    // Models/User.cs
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }

        // ── Naye Fields ──────────────────────────
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Pincode { get; set; }
        public string? ProfileImage { get; set; }
        public bool IsActive { get; set; } = true;

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public List<Order>? Orders { get; set; }
        public List<Cart>? Carts { get; set; }
    }
}
