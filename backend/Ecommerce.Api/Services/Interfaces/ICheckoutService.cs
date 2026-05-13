using Ecommerce.Api.DTOs.Checkout;

namespace Ecommerce.Api.Services.Interfaces;

public interface ICheckoutService
{
    Task<CheckoutResponseDto> PlaceOrderAsync(
        Guid userId,
        CheckoutRequestDto request,
        CancellationToken cancellationToken = default);
}
