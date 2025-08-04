namespace InvoiceSystem.ViewModels
{
    public class PaymentViewModel
    {
        public int OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
    }
}
