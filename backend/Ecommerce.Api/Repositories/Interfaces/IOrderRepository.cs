using Ecommerce.Api.Models;

namespace Ecommerce.Api.Repositories.Interfaces;

public interface IOrderRepository
{
    Task CreateAsync(
        Order order,
        IReadOnlyList<OrderItem> orderItems,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Order?> GetByIdForUserAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderItem>> GetItemsByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);
}
