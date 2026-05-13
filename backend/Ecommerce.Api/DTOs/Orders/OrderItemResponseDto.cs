namespace Ecommerce.Api.DTOs.Orders;

public sealed record OrderItemResponseDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);
