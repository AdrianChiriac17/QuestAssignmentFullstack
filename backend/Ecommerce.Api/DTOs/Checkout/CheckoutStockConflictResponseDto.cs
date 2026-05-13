namespace Ecommerce.Api.DTOs.Checkout;

public sealed record CheckoutStockConflictResponseDto(
    string Message,
    IReadOnlyList<CheckoutStockConflictItemResponseDto> Errors);
