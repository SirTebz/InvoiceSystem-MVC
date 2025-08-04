using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceSystem.Models
{
    public class Order
    {
        public int Id { get; set; }
        // Foreign key and navigation property for Customer
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        
        // Collection of OrderItems
        // Navigation property: an order can have multiple order items.
        public ICollection<OrderItem> OrderItems { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        // e.g., Pending, Completed, Canceled 
        public string OrderStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        // Navigation property for Payment (one-to-one relationship)
        public Payment Payment { get; set; }
    }
}
