using Ecommerce.Api.Models;

namespace Ecommerce.Api.DTOs.Products;

public sealed record ProductSizeStockResponseDto(
    ProductSize Size,
    int StockQuantity);
