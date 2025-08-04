using InvoiceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Customers (2 regular and 1 admin).
            modelBuilder.Entity<Customer>().HasData(
                new Customer { Id = 1, CustomerName = "Teboho Mokgosi", Email = "mokgositeboho77@gmail.com", Password = "Password123", Phone = "1234567890", BillingAddress = "123 Main Str" },
                new Customer { Id = 2, CustomerName = "John Doe", Email = "johndoe@gmail.com", Password = "Password123", Phone = "0987654321", BillingAddress = "456 Park West" },
                new Customer { Id = 3, CustomerName = "Admin", Email = "admin@example.com", Password = "admin", Phone = "1112223333", BillingAddress = "Admin HQ" }
            );

            // Seed Products.
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    ProductName = "Apple iPhone 16",
                    Category = "Smartphones",
                    Description = "The latest Apple iPhone featuring the A18 Bionic chip, 5G connectivity, and an advanced dual-camera system.",
                    UnitPrice = 18499.99m
                },
                new Product
                {
                    Id = 2,
                    ProductName = "Samsung Galaxy S25",
                    Category = "Smartphones",
                    Description = "A high-end smartphone with a 6.3-inch display, Snapdragon Elite processor, and a versatile triple-camera setup.",
                    UnitPrice = 15699.00m
                },
                new Product
                {
                    Id = 3,
                    ProductName = "Sony WH-1000XM4 Headphones",
                    Category = "Audio",
                    Description = "Industry-leading noise-canceling headphones with superior sound quality and long battery life.",
                    UnitPrice = 1349.99m
                },
                new Product
                {
                    Id = 4,
                    ProductName = "Dell XPS 13 Laptop",
                    Category = "Computers",
                    Description = "A sleek and powerful ultrabook featuring a 13.4-inch display, Intel Core i7 processor, and fast SSD storage.",
                    UnitPrice = 18999.99m
                },
                new Product
                {
                    Id = 5,
                    ProductName = "Xbox Series S 512 GB",
                    Category = "Video Game Consoles",
                    Description = "a digital-focused, next-generation gaming console known for its compact size, delivering high-speed performance with a custom SSD and competitive price point.",
                    UnitPrice = 7999.99m
                }
            );
        }

        public DbSet<Template> Templates { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
    }
}
