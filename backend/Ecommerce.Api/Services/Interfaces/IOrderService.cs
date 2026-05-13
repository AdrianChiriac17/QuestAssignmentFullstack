using Ecommerce.Api.DTOs.Orders;

namespace Ecommerce.Api.Services.Interfaces;

public interface IOrderService
{
    Task<OrdersResponseDto> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<OrderResponseDto?> GetByIdForUserAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
