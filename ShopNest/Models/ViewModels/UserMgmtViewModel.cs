namespace ShopNest.Models.ViewModels
{
    public class UserMgmtViewModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }

        public string Initials
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) return "U";

                var parts = Name.Trim().Split(' ');

                // ← Safe check
                if (parts.Length >= 2 &&
                    parts[1].Length > 0)
                    return $"{parts[0][0]}{parts[1][0]}"
                           .ToUpper();

                // Sirf ek word hai
                if (parts[0].Length >= 2)
                    return parts[0].Substring(0, 2).ToUpper();

                return parts[0].Substring(0, 1).ToUpper();
            }
        }

        public string FormattedDate => CreatedAt.ToString("MMM dd, yyyy");

        public string StatusBadgeClass => IsActive ? "bdg-active" : "bdg-inactive";
        public string StatusText => IsActive ? "Active" : "Inactive";

        public string AvatarStyle
        {
            get
            {
                int mod = UserId % 5;
                if (mod == 0) return "background:var(--blue-l);color:var(--blue)";
                if (mod == 1) return "background:#d1fae5;color:var(--emerald)";
                if (mod == 2) return "background:#fef3c7;color:var(--amber)";
                if (mod == 3) return "background:#ffe4e6;color:var(--rose)";
                return "background:#ede9fe;color:var(--violet)";
            }
        }
    }
}
