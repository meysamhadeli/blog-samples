using Catalog.Products.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Shared.Data;

public class CatalogsDbContext(DbContextOptions<CatalogsDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
