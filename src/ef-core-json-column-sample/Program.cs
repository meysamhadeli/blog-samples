using System.Reflection;
using ef.core.json.column.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseNpgsql(
        "Server=localhost;Port=5432;Database=product;User Id=postgres;Password=postgres;Include Error Detail=true",
        dbOptions => { dbOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name); });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    DataSeeder.SeedProducts(context);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();