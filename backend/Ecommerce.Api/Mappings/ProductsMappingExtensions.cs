using Ecommerce.Api.DTOs.Products;
using Ecommerce.Api.Models;

namespace Ecommerce.Api.Mappings;

public static class ProductsMappingExtensions
{
    public static ProductSizeStockResponseDto ToResponseDto(this ProductSizeStock sizeStock)
    {
        return new ProductSizeStockResponseDto(
            sizeStock.Size,
            sizeStock.StockQuantity);
    }

    public static ProductResponseDto ToResponseDto(
        this Product product,
        IReadOnlyList<ProductSizeStock> sizeStocks)
    {
        return new ProductResponseDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.FrontImageUrl,
            product.BackImageUrl,
            sizeStocks.Select(sizeStock => sizeStock.ToResponseDto()).ToList());
    }

    public static ProductsResponseDto ToProductsResponseDto(this IReadOnlyList<ProductResponseDto> products)
    {
        return new ProductsResponseDto(products);
    }
}
