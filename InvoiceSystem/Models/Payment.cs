using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }
        // e.g., "CreditCard", "PayPal", "COD", etc.
        public string PaymentMethod { get; set; }
        // Pending, Success or Failed
        public string PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
