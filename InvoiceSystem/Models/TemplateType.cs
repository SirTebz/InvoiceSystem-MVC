namespace InvoiceSystem.Models
{
    public enum TemplateType
    {
        OrderCompletion = 1,
        OrderCancelled = 2,
        OrderPending = 3,
        PaymentSuccess = 4,
        PaymentFailure = 5,
        PaymentPending = 6,
        CustomerRegistration = 7
    }
}
