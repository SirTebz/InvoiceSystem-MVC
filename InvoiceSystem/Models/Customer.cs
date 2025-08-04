namespace InvoiceSystem.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        // For simplicity, password is stored plain text.
        public string Password { get; set; }
        public string Phone { get; set; }
        public string BillingAddress { get; set; }
        public List<Order> Orders { get; set; }
    }
}
