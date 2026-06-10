namespace ShopNest.Models.ViewModels
{
    // Models/ViewModels/PaymentViewModel.cs
  
    public class PaymentViewModel
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string RazorpayOrderId { get; set; }
        public string KeyId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }

        // COD ke liye
        public decimal CODFee => 50m; // ← COD fee
        public decimal TotalWithCOD => Amount + CODFee;
    }
}
