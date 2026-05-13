namespace Ecommerce.Api.DTOs.Products;

public sealed record ProductResponseDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string FrontImageUrl,
    string? BackImageUrl,
    IReadOnlyList<ProductSizeStockResponseDto> Sizes);
