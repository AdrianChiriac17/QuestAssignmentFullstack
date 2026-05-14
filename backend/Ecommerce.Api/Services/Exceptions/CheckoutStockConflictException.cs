using Ecommerce.Api.DTOs.Errors;

namespace Ecommerce.Api.Services.Exceptions;

public class CheckoutStockConflictException : Exception
{
    public CheckoutStockConflictException(
        IReadOnlyList<CheckoutStockConflictItemResponseDto> errors)
        : base("Some items are no longer available in the requested quantity.")
    {
        Response = new CheckoutStockConflictResponseDto(Message, errors);
    }

    public CheckoutStockConflictResponseDto Response { get; }
}
