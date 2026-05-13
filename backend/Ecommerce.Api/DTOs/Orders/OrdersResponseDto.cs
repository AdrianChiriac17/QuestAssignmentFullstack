namespace Ecommerce.Api.DTOs.Orders;

public sealed record OrdersResponseDto(
    IReadOnlyList<OrderSummaryResponseDto> Orders);
