using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YunChenShipping.Models;

namespace YunChenShipping.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<CustomerContact> CustomerContacts { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductPriceHistory> ProductPriceHistories { get; set; }
        public DbSet<ShippingOrder> ShippingOrders { get; set; }
        public DbSet<ShippingOrderDetail> ShippingOrderDetails { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 客戶配置
            builder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Phone);
                entity.HasIndex(e => e.TaxId);
            });

            // 客戶地址配置
            builder.Entity<CustomerAddress>(entity =>
            {
                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 客戶聯絡人配置
            builder.Entity<CustomerContact>(entity =>
            {
                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Contacts)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 產品配置
            builder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => e.PartNo);
                entity.HasIndex(e => e.Name);
            });

            // 產品價格歷史配置
            builder.Entity<ProductPriceHistory>(entity =>
            {
                entity.HasOne(d => d.Product)
                    .WithMany(p => p.PriceHistory)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 出貨單配置
            builder.Entity<ShippingOrder>(entity =>
            {
                entity.HasIndex(e => e.OrderNo).IsUnique();
                entity.HasIndex(e => e.OrderDate);
                entity.HasIndex(e => e.Status);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.ShippingOrders)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 出貨單明細配置
            builder.Entity<ShippingOrderDetail>(entity =>
            {
                entity.HasOne(d => d.ShippingOrder)
                    .WithMany(p => p.Details)
                    .HasForeignKey(d => d.ShippingOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Product)
                    .WithMany()
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
