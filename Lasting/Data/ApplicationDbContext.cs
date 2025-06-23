using System.Drawing.Drawing2D;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Lasting.Models;

namespace Lasting.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>().OwnsOne(
                o => o.ShippingAddress, sa =>
                {
                    sa.Property(p => p.FullName).HasColumnName("ShippingFullName");
                    sa.Property(p => p.AddressLine1).HasColumnName("ShippingAddress1");
                    sa.Property(p => p.AddressLine2).HasColumnName("ShippingAddress2");
                    sa.Property(p => p.City).HasColumnName("ShippingCity");
                    sa.Property(p => p.PhoneNumber).HasColumnName("ShippingPhone");
                });
        }

    }
}