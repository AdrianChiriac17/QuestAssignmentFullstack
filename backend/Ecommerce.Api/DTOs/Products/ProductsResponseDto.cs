namespace Ecommerce.Api.DTOs.Products;

public sealed record ProductsResponseDto(
    IReadOnlyList<ProductResponseDto> Products);
