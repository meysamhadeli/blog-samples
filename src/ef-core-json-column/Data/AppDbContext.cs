using ef.core.json.column.Models;
using Microsoft.EntityFrameworkCore;

namespace ef.core.json.column.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<Product> Products => Set<Product>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<Product>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired();
            b.OwnsMany(x => x.SupplierInformations, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToJson(); // Stores the collection as a JSON column
                ownedNavigationBuilder.OwnsOne(s => s.Address); // Configure the owned type Address
            });
        });

        base.OnModelCreating(modelBuilder);
    }
}