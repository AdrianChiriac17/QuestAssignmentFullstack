using Ecommerce.Api.DTOs.Orders;
using Ecommerce.Api.Models;

namespace Ecommerce.Api.Mappings;

public static class OrdersMappingExtensions
{
    public static OrderSummaryResponseDto ToSummaryResponseDto(this Order order)
    {
        return new OrderSummaryResponseDto(
            order.Id,
            order.TotalPrice,
            order.CreatedAt);
    }

    public static OrderItemResponseDto ToResponseDto(this OrderItem orderItem)
    {
        return new OrderItemResponseDto(
            orderItem.ProductId,
            orderItem.ProductName,
            orderItem.Size,
            orderItem.UnitPrice,
            orderItem.Quantity,
            orderItem.LineTotal);
    }

    public static OrderResponseDto ToResponseDto(
        this Order order,
        IReadOnlyList<OrderItem> orderItems)
    {
        return new OrderResponseDto(
            order.Id,
            order.RecipientName,
            order.AddressLine,
            order.City,
            order.PostalCode,
            order.Country,
            order.TotalPrice,
            order.CreatedAt,
            orderItems.Select(orderItem => orderItem.ToResponseDto()).ToList());
    }

    public static OrdersResponseDto ToOrdersResponseDto(this IReadOnlyList<Order> orders)
    {
        return new OrdersResponseDto(
            orders.Select(order => order.ToSummaryResponseDto()).ToList());
    }
}
