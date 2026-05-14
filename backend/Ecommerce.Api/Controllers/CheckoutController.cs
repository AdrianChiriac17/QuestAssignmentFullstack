using System.Security.Claims;
using Ecommerce.Api.DTOs.Checkout;
using Ecommerce.Api.DTOs.Errors;
using Ecommerce.Api.Services.Exceptions;
using Ecommerce.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/checkout")]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        this.checkoutService = checkoutService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CheckoutResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CheckoutStockConflictResponseDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CheckoutResponseDto>> PlaceOrder(
        CheckoutRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized(new ApiErrorResponseDto("Invalid authentication token."));
        }

        try
        {
            var response = await checkoutService.PlaceOrderAsync(
                userId.Value,
                request,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (CheckoutStockConflictException exception)
        {
            return Conflict(exception.Response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ApiErrorResponseDto(exception.Message));
        }
    }

    private Guid? GetUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
