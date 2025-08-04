using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace InvoiceSystem.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [DisplayName("Full Name")]
        public string CustomerName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Phone { get; set; }
        [DisplayName("Billing Address")]
        public string BillingAddress { get; set; }
    }
}
