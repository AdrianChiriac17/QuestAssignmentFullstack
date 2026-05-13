namespace Ecommerce.Api.DTOs.Orders;

public sealed record OrderResponseDto(
    Guid Id,
    string RecipientName,
    string AddressLine,
    string City,
    string PostalCode,
    string Country,
    decimal TotalPrice,
    DateTimeOffset CreatedAt,
    IReadOnlyList<OrderItemResponseDto> Items);
