using Microsoft.EntityFrameworkCore;
using MyPos.API.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MyPos.API.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ShopSetting> ShopSettings { get; set; }
        public DbSet<DeviceApprovalRequest> DeviceApprovalRequests { get; set; }
        public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Product> Products { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>(e =>
            {
                e.ToTable("Clients");
                e.HasKey(x => x.ClientID);
            });

            modelBuilder.Entity<Role>(e =>
            {
                e.ToTable("Roles");
                e.HasKey(x => x.RoleId);
            });

            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasKey(x => x.UserId);
                e.HasIndex(x => new { x.ClientID, x.UserName }).IsUnique();

                e.HasOne(x => x.Client)
                    .WithMany(c => c.Users)
                    .HasForeignKey(x => x.ClientID)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ShopSetting>(e =>
            {
                e.ToTable("ShopSettings");
                e.HasKey(x => x.ShopSettingId);

                e.HasOne(x => x.Client)
                    .WithMany(c => c.ShopSettings)
                    .HasForeignKey(x => x.ClientID)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<DeviceApprovalRequest>(e =>
            {
                e.ToTable("DeviceApprovalRequests");
                e.HasKey(x => x.RequestId);
                e.Property(x => x.DeviceId).HasMaxLength(200);
                e.Property(x => x.Platform).HasMaxLength(20);
                e.Property(x => x.Status).HasMaxLength(20);

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Client)
                    .WithMany()
                    .HasForeignKey(x => x.ClientID)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Category>(e =>
            {
                e.ToTable("Categories");
                e.HasKey(x => x.CategoryId);
            });

            modelBuilder.Entity<Brand>(e =>
            {
                e.ToTable("Brands");
                e.HasKey(x => x.BrandId);
            });

            modelBuilder.Entity<Unit>(e =>
            {
                e.ToTable("Units");
                e.HasKey(x => x.UnitId);
            });

            modelBuilder.Entity<Product>(e =>
            {
                e.ToTable("Products");
                e.HasKey(x => x.ProductId);
                e.Property(x => x.PurchaseRate).HasPrecision(18, 2);
                e.Property(x => x.SaleRate).HasPrecision(18, 2);
                e.Property(x => x.MinimumStock).HasPrecision(18, 3);
                e.Property(x => x.TaxPercent).HasPrecision(5, 2);
            });
        }
    }
}