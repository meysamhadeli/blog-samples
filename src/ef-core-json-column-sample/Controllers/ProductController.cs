using ef.core.json.column.Data;
using ef.core.json.column.Dtos;
using ef.core.json.column.Models.ValueObjects;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ef.core.json.column.Controllers;

[ApiController]
[Route("product")]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProductController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("get-by-country")]
    public async Task<ActionResult> GetProductByCountry([FromQuery] string country = "china")
    {
        var result = await _dbContext.Products
            .Where(p => p.SupplierInformations!.Any(s => s.Address.Country == country)).ToListAsync();

        return Ok(result);
    }


    [HttpPut("update")]
    public async Task<ActionResult> UpdateProduct([FromQuery] int id, [FromBody] ProductRequestDto productRequestDto)
    {
        // Retrieve the product by its ID
        var product = _dbContext.Products.FirstOrDefault(p => p.Id == id);

        if (product != null)
        {
            product.Name = productRequestDto.Name;
            product.Description = productRequestDto.Description;
            product.SupplierInformations =
                productRequestDto.SupplierInformations.Adapt<ICollection<SupplierInformation>>();

            // Save the changes to the database
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();
        }

        return NoContent();
    }
}