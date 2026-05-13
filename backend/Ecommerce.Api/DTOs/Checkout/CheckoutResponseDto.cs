namespace Ecommerce.Api.DTOs.Checkout;

public sealed record CheckoutResponseDto(
    Guid OrderId,
    decimal TotalPrice,
    DateTimeOffset CreatedAt);
