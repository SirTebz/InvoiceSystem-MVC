using InvoiceSystem.Data;
using InvoiceSystem.Models;
using InvoiceSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InvoiceSystem.Controllers
{
    public class AccountController : Controller
    {
        // Access the database.
        private readonly ApplicationDbContext _context;

        // Constructor that receives the DbContext via dependency injection.
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Register
        // Registration page.
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Customers.AnyAsync(c => c.Email == model.Email))
                {
                    ModelState.AddModelError("", "Email already registered.");
                    return View(model); 
                }

                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    Email = model.Email,
                    Password = model.Password, // dummy password.
                    Phone = model.Phone,
                    BillingAddress = model.BillingAddress
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: Account/Login
        // Login page.
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = _context.Customers.AsNoTracking()
                    .FirstOrDefault(c => c.Email == model.Email && c.Password == model.Password);
                if (customer != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                        new Claim(ClaimTypes.Name, customer.CustomerName),
                        new Claim(ClaimTypes.Email, customer.Email)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));
                    return RedirectToAction("Index", "Order");
                }
                ModelState.AddModelError("", "Invalid login attempt.");
            }

            return View(model);
        }

        // GET: Account/Logout
        // Logs out the user.
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Redirect to the Login page after logout.
            return RedirectToAction("Login");
        }
    }
}
