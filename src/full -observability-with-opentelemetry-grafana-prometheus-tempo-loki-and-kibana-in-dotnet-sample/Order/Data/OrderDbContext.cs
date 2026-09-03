using Microsoft.EntityFrameworkCore;
using Order.Orders.Models;

namespace Order.Data;

public sealed class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Orders.Models.Order> Orders => Set<Orders.Models.Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Orders.Models.Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.CustomerName).HasMaxLength(100).IsRequired();
            entity.Property(o => o.ShippingAddress).HasMaxLength(500);
            entity.Property(o => o.Status).HasMaxLength(20).IsRequired();
            entity.Property(o => o.TotalPrice).HasColumnType("decimal(18,2)");
            entity.HasMany(o => o.Items)
                  .WithOne(i => i.Order)
                  .HasForeignKey(i => i.OrderId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ProductName).HasMaxLength(100).IsRequired();
            entity.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        });
    }
}
