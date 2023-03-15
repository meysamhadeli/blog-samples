using AutoMapper;
using Catalog.Products.Features.CreatingProduct;
using Catalog.Products.Models;

namespace Catalog.Products.Features;

public class ProductMappings: Profile
{
    public ProductMappings()
    {
        CreateMap<GetProductByIdEndpoint.CreateProductRequestDto, CreateProduct.CreateProductCommand>();
        CreateMap<CreateProduct.CreateProductCommand, Product>();
        CreateMap<Product, CreateProduct.CreateProductResult>();
    }
}