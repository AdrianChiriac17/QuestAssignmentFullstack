using Ecommerce.Api.DTOs.Orders;
using Ecommerce.Api.Mappings;
using Ecommerce.Api.Repositories.Interfaces;
using Ecommerce.Api.Services.Interfaces;

namespace Ecommerce.Api.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    public async Task<OrdersResponseDto> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.GetByUserIdAsync(userId, cancellationToken);

        return orders.ToOrdersResponseDto();
    }

    public async Task<OrderResponseDto?> GetByIdForUserAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdForUserAsync(orderId, userId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        var orderItems = await orderRepository.GetItemsByOrderIdAsync(order.Id, cancellationToken);

        return order.ToResponseDto(orderItems);
    }
}
