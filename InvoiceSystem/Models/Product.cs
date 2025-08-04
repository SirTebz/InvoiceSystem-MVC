using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceSystem.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public string? Description { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
