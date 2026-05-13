using Ecommerce.Api.Models;

namespace Ecommerce.Api.DTOs.Orders;

public sealed record OrderItemResponseDto(
    Guid ProductId,
    string ProductName,
    ProductSize Size,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);
