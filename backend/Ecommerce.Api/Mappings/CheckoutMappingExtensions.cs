using Ecommerce.Api.DTOs.Checkout;
using Ecommerce.Api.DTOs.Errors;
using Ecommerce.Api.Models;

namespace Ecommerce.Api.Mappings;

public static class CheckoutMappingExtensions
{
    public static Order ToOrder(this CheckoutRequestDto request, Guid userId, decimal totalPrice)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RecipientName = request.RecipientName,
            AddressLine = request.AddressLine,
            City = request.City,
            PostalCode = request.PostalCode,
            Country = request.Country,
            TotalPrice = totalPrice,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public static CheckoutResponseDto ToCheckoutResponseDto(this Order order)
    {
        return new CheckoutResponseDto(
            order.Id,
            order.TotalPrice,
            order.CreatedAt);
    }

    public static CheckoutStockConflictItemResponseDto ToCheckoutStockConflictItemResponseDto(
        this CheckoutItemRequestDto item,
        int availableQuantity)
    {
        return new CheckoutStockConflictItemResponseDto(
            item.ProductId,
            item.Size,
            item.Quantity,
            availableQuantity);
    }
}
