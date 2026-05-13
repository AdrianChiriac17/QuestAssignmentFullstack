namespace Ecommerce.Api.DTOs.Errors;

public sealed record CheckoutStockConflictResponseDto(
    string Message,
    IReadOnlyList<CheckoutStockConflictItemResponseDto> Errors);
