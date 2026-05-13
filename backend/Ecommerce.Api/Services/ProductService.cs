using Ecommerce.Api.DTOs.Products;
using Ecommerce.Api.Mappings;
using Ecommerce.Api.Models;
using Ecommerce.Api.Repositories.Interfaces;
using Ecommerce.Api.Services.Interfaces;

namespace Ecommerce.Api.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository productRepository;

    public ProductService(IProductRepository productRepository)
    {
        this.productRepository = productRepository;
    }

    public async Task<ProductsResponseDto> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);
        var productIds = products.Select(product => product.Id).ToList();
        var sizeStocks = await productRepository.GetSizeStocksByProductIdsAsync(
            productIds,
            cancellationToken);

        var sizeStocksByProductId = sizeStocks
            .GroupBy(sizeStock => sizeStock.ProductId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<ProductSizeStock>)group.ToList());

        var productResponseDtos = products
            .Select(product => product.ToResponseDto(
                sizeStocksByProductId.GetValueOrDefault(product.Id, [])))
            .ToList();

        return productResponseDtos.ToProductsResponseDto();
    }
}
