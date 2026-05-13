using Ecommerce.Api.Models;

namespace Ecommerce.Api.DTOs.Errors;

public sealed record CheckoutStockConflictItemResponseDto(
    Guid ProductId,
    ProductSize Size,
    int RequestedQuantity,
    int AvailableQuantity);
