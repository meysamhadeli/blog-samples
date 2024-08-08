using ef.core.json.column.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ef.core.json.column.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProductController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("get-product-by-country")]
    public async Task<ActionResult> GetProductByCountry([FromQuery] string country = "china")
    {
        var productsWithChinaSupplier = await _dbContext.Products
            .Where(p => p.SupplierInformations!.Any(s => s.Address.Country == country)).ToListAsync();

        return Ok(productsWithChinaSupplier);
    }
}