using ef.core.json.column.Models;
using ef.core.json.column.Models.ValueObjects;

namespace ef.core.json.column.Data;

public class DataSeeder
{
    public static void SeedProducts(AppDbContext context)
    {
        if (!context.Products.Any())
        {
            var products = new List<Product>
            {
                new Product
                {
                    Name = "test-product",
                    Description = "test-product-description",
                    SupplierInformations = new List<SupplierInformation>()
                    {
                        new SupplierInformation
                        {
                            Name = "canon",
                            Address = new Address
                            {
                                Country = "china",
                                City = "bejing",
                                Street = "bejing str"
                            }
                        },
                        new SupplierInformation
                        {
                            Name = "fujifilm",
                            Address = new Address
                            {
                                Country = "china",
                                City = "shanghai",
                                Street = "shanghai str"
                            }
                        }
                    }
                },
                new Product
                {
                    Name = "test-product-2",
                    Description = "test-product-description-2",
                    SupplierInformations = new List<SupplierInformation>()
                    {
                        new SupplierInformation
                        {
                            Name = "nikon",
                            Address = new Address
                            {
                                Country = "korea",
                                City = "seoul",
                                Street = "seoul str"
                            }
                        }
                    }
                }
            };

            context.AddRange(products);
            context.SaveChanges();
        }
    }
}