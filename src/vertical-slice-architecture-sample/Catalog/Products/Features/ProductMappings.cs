using AutoMapper;
using Catalog.Products.Features.CreatingProduct;
using Catalog.Products.Models;

namespace Catalog.Products.Features;

public class ProductMappings: Profile
{
    public ProductMappings()
    {
        CreateMap<CreateProductEndpoint.CreateProductRequestDto, CreateProduct>();
        CreateMap<CreateProduct, Product>();
        CreateMap<Product, CreateProductResult>();
    }
}