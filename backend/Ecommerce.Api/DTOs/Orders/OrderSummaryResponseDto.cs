namespace Ecommerce.Api.DTOs.Orders;

public sealed record OrderSummaryResponseDto(
    Guid Id,
    decimal TotalPrice,
    DateTimeOffset CreatedAt);
