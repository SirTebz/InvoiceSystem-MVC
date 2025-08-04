using InvoiceSystem.Data;
using InvoiceSystem.Models;
using InvoiceSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace InvoiceSystem.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public OrderController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (customerIdClaim == null)
                return RedirectToAction("Login", "Account");
            int customerId = int.Parse(customerIdClaim.Value);
            var orders = await _context.Orders.AsNoTracking().Include(o => o.OrderItems).Where(o => o.CustomerId == customerId).ToListAsync();

            return View(orders);
        }

        // GET: Order/Create
        // Displays the order creation form.
        public IActionResult Create()
        {
            ViewBag.Products = _context.Products.AsNoTracking().ToList();
            return View();
        }

        // POST: Order/Create
        // Processes the submitted order creation form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (customerIdClaim == null)
                return RedirectToAction("Login", "Account");
            int customerId = int.Parse(customerIdClaim.Value);
            var orderNumber = $"ORD-{DateTime.UtcNow.Ticks}";
            var order = new Order
            {
                CustomerId = customerId,
                OrderNumber = orderNumber,
                OrderDate = DateTime.UtcNow,
                OrderStatus = "Pending", // New orders start with "Pending" status.
                CreatedDate = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };
            decimal totalAmount = 0;

            foreach (var item in model.OrderItems)
            {
                if (item.Quantity <= 0)
                    continue;
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    continue;
                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.UnitPrice
                };
                totalAmount += orderItem.Total;
                order.OrderItems.Add(orderItem);
            }
            order.TotalAmount = totalAmount;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Payment", new { orderId = order.Id });
        }

        // GET: Order/Payment
        // Displays the payment page.
        public async Task<IActionResult> Payment(int orderId)
        {
            var paymentViewModel = await _context.Orders.AsNoTracking()
            .Select(order => new PaymentViewModel()
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount
            })
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (paymentViewModel == null)
                return NotFound();
            //ViewBag.Order = order;
            return View(paymentViewModel);
        }

        // POST: Order/Payment
        // Processes the payment for the order.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(PaymentViewModel paymentViewModel)
        {
            var order = await _context.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == paymentViewModel.OrderId);
            if (order == null)
                return NotFound();
            string paymentStatus = "";
            if (paymentViewModel.PaymentMethod.ToUpper() == "PAYPAL")
            {
                order.OrderStatus = "Cancelled";
                paymentStatus = "Failed";
            }
            else if (paymentViewModel.PaymentMethod.ToUpper() == "COD")
            {
                order.OrderStatus = "Pending";
                paymentStatus = "Pending";
            }
            else
            {
                order.OrderStatus = "Completed";
                paymentStatus = "Success";
            }
            var payment = new Payment
            {
                OrderId = order.Id,
                AmountPaid = order.TotalAmount,
                PaymentMethod = paymentViewModel.PaymentMethod,
                PaymentStatus = paymentStatus,
                PaymentDate = DateTime.UtcNow
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            byte[] pdfBytes = await GenerateInvoicePdfAsync(order.Id);
            await SendInvoiceEmail(order.Customer.Email, order.Customer.CustomerName,
            BuildEmailSubject(order),
            BuildEmailBody(order),
            pdfBytes);

            return RedirectToAction("OrderPlaced", new { orderId = order.Id });
        }

        // GET: Order/OrderPlaced/{orderId}
        // Displays a confirmation page with the order details after the order is placed.
        public async Task<IActionResult> OrderPlaced(int orderId)
        {
            var order = await _context.Orders.AsNoTracking()
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                return NotFound();

            return View(order);
        }
        // GET: Order/GenerateInvoice/{orderId}?sendEmail=false
        // Generates a PDF invoice for the given order.
        [AllowAnonymous]
        public async Task<IActionResult> GenerateInvoice(int orderId, bool sendEmail = false)
        {
            byte[] pdfBytes = await GenerateInvoicePdfAsync(orderId);
            if (sendEmail)
            {
                var order = await _context.Orders.AsNoTracking()
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == orderId);
                if (order != null)
                {
                    await SendInvoiceEmail(order.Customer.Email, order.Customer.CustomerName,
                    BuildEmailSubject(order),
                    BuildEmailBody(order),
                    pdfBytes);
                }
            }
            return File(pdfBytes, "application/pdf", $"OrderInvoice_{orderId}.pdf");
        }

        private async Task<byte[]> GenerateInvoicePdfAsync(int orderId)
        {
            var order = await _context.Orders.AsNoTracking()
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.Customer)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                throw new Exception("Order not found.");
            TemplateType templateType;
            if (order.OrderStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                templateType = TemplateType.OrderCompletion;
            }
            else if (order.OrderStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                templateType = TemplateType.OrderCancelled;
            }
            else
            {
                templateType = TemplateType.OrderPending;
            }

            var template = await _context.Templates.FirstOrDefaultAsync(t => t.TemplateType == templateType);
            if (template == null)
                throw new Exception("Invoice template not found.");

            string productListHtml = "";
            foreach (var item in order.OrderItems)
            {
                productListHtml += $"<tr>" +
                $"<td style='padding:12px; border:1px solid #ddd;'>{item.Product.ProductName}</td>" +
                $"<td style='padding:12px; border:1px solid #ddd;'>{item.Quantity}</td>" +
                $"<td style='padding:12px; border:1px solid #ddd;'>{item.UnitPrice:C}</td>" +
                $"<td style='padding:12px; border:1px solid #ddd;'>{item.Total:C}</td>" +
                $"</tr>";
            }
            productListHtml += "</tbody></table>";
            string htmlContent = template.HtmlContent;
            htmlContent = ReplacePlaceholder(htmlContent, "CustomerName", order.Customer.CustomerName);
            htmlContent = ReplacePlaceholder(htmlContent, "OrderDate", order.OrderDate.ToString("MMMM dd, yyyy"));
            htmlContent = ReplacePlaceholder(htmlContent, "OrderNumber", order.OrderNumber);
            htmlContent = ReplacePlaceholder(htmlContent, "OrderStatus", order.OrderStatus);
            htmlContent = ReplacePlaceholder(htmlContent, "ProductList", productListHtml);
            htmlContent = ReplacePlaceholder(htmlContent, "TotalAmount", order.TotalAmount.ToString("C"));
            if (order.Payment != null)
            {
                htmlContent = ReplacePlaceholder(htmlContent, "PaymentStatus", order.Payment.PaymentStatus);
                htmlContent = ReplacePlaceholder(htmlContent, "PaymentMethod", order.Payment.PaymentMethod);
                htmlContent = ReplacePlaceholder(htmlContent, "PaymentDate", order.Payment.PaymentDate.ToString("MMMM dd, yyyy"));
            }
            var renderer = new ChromePdfRenderer();
            var pdfDocument = renderer.RenderHtmlAsPdf(htmlContent);
            return pdfDocument.BinaryData;
        }

        // Helper method for placeholder replacement.
        private string ReplacePlaceholder(string html, string placeholder, string value)
        {
            return html.Replace("{" + placeholder + "}", value);
        }

        private string BuildEmailSubject(Order order)
        {
            if (order.OrderStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                return $"Your Order Invoice - Order #{order.OrderNumber} Completed";
            else if (order.OrderStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                return $"Your Order Invoice - Order #{order.OrderNumber} Cancelled";
            else
                return $"Your Order Invoice - Order #{order.OrderNumber} Pending";
        }

        private string BuildEmailBody(Order order)
        {
            if (order.OrderStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                return $"Dear {order.Customer.CustomerName},<br/><br/>" +
                $"Thank you for your order. Your order #{order.OrderNumber} has been completed successfully. " +
                $"Please find attached your invoice.<br/><br/>Best regards,<br/>My Estore App";
            }
            else if (order.OrderStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return $"Dear {order.Customer.CustomerName},<br/><br/>" +
                $"We regret to inform you that your order #{order.OrderNumber} has been cancelled. " +
                $"Please contact our support for further details.<br/><br/>Best regards,<br/>My Estore App";
            }
            else
            {
                return $"Dear {order.Customer.CustomerName},<br/><br/>" +
                $"Please find attached your invoice for order #{order.OrderNumber}.<br/><br/>Best regards,<br/>My Estore App";
            }
        }

        // Helper method to send the invoice PDF via email using SMTP.
        private async Task SendInvoiceEmail(string toEmail, string customerName, string subject, string htmlBody, byte[] pdfAttachment, bool isBodyHtml = true)
        {
            string smtpServer = _configuration.GetValue<string>("EmailSettings:SmtpServer") ?? "";
            int smtpPort = int.Parse(_configuration.GetValue<string>("EmailSettings:SmtpPort") ?? "587");
            string senderName = _configuration.GetValue<string>("EmailSettings:SenderName") ?? "My Estore App";
            string senderEmail = _configuration.GetValue<string>("EmailSettings:SenderEmail") ?? "";
            string password = _configuration.GetValue<string>("EmailSettings:Password") ?? "";
            using (var message = new MailMessage())
            {
                message.From = new MailAddress(senderEmail, senderName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = htmlBody;
                message.IsBodyHtml = isBodyHtml;
                message.Attachments.Add(new Attachment(new MemoryStream(pdfAttachment), "OrderInvoice.pdf", "application/pdf"));
                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.Credentials = new NetworkCredential(senderEmail, password);
                    client.EnableSsl = true;
                    // Send the email asynchronously.
                    await client.SendMailAsync(message);
                }
            }
        }
    }
}
