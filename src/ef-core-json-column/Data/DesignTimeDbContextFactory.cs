using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ef.core.json.column.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();

        builder.UseNpgsql("Server=localhost;Port=5432;Database=product;User Id=postgres;Password=postgres;Include Error Detail=true");

        return new AppDbContext(builder.Options);
    }
}