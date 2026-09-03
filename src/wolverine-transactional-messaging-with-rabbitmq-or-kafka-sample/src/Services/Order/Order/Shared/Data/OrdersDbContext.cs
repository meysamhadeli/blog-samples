using Microsoft.EntityFrameworkCore;
using Order.Products.Models;

namespace Order.Shared.Data;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<ImportedProduct> ImportedProducts => Set<ImportedProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
