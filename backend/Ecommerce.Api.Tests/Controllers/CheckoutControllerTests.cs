using Ecommerce.Api.Controllers;
using Ecommerce.Api.DTOs.Checkout;
using Ecommerce.Api.DTOs.Errors;
using Ecommerce.Api.Models;
using Ecommerce.Api.Services.Exceptions;
using Ecommerce.Api.Services.Interfaces;
using Ecommerce.Api.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace Ecommerce.Api.Tests.Controllers;

public class CheckoutControllerTests
{
    [Fact]
    public async Task PlaceOrder_ReturnsCreatedWhenCheckoutSucceeds()
    {
        var userId = Guid.NewGuid();
        var request = CreateRequest();
        var response = new CheckoutResponseDto(Guid.NewGuid(), 119.98m, DateTimeOffset.UtcNow);
        var checkoutService = new Mock<ICheckoutService>();
        var controller = new CheckoutController(checkoutService.Object)
        {
            ControllerContext = ControllerTestHelper.CreateControllerContext(userId)
        };

        checkoutService
            .Setup(service => service.PlaceOrderAsync(
                userId,
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await controller.PlaceOrder(request, CancellationToken.None);

        var statusResult = result.Result.ShouldBeOfType<ObjectResult>();
        statusResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
        statusResult.Value.ShouldBe(response);
    }

    [Fact]
    public async Task PlaceOrder_ReturnsUnauthorizedWhenUserIdClaimIsMissing()
    {
        var checkoutService = new Mock<ICheckoutService>();
        var controller = new CheckoutController(checkoutService.Object)
        {
            ControllerContext = ControllerTestHelper.CreateControllerContextWithoutUserId()
        };

        var result = await controller.PlaceOrder(CreateRequest(), CancellationToken.None);

        var unauthorizedResult = result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
        var error = unauthorizedResult.Value.ShouldBeOfType<ApiErrorResponseDto>();
        error.Message.ShouldBe("Invalid authentication token.");
        checkoutService.Verify(
            service => service.PlaceOrderAsync(
                It.IsAny<Guid>(),
                It.IsAny<CheckoutRequestDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PlaceOrder_ReturnsConflictWhenStockConflictOccurs()
    {
        var userId = Guid.NewGuid();
        var request = CreateRequest();
        var conflictItem = new CheckoutStockConflictItemResponseDto(
            request.Items[0].ProductId,
            ProductSize.M,
            2,
            1);
        var checkoutService = new Mock<ICheckoutService>();
        var controller = new CheckoutController(checkoutService.Object)
        {
            ControllerContext = ControllerTestHelper.CreateControllerContext(userId)
        };

        checkoutService
            .Setup(service => service.PlaceOrderAsync(
                userId,
                request,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CheckoutStockConflictException([conflictItem]));

        var result = await controller.PlaceOrder(request, CancellationToken.None);

        var conflictResult = result.Result.ShouldBeOfType<ConflictObjectResult>();
        var response = conflictResult.Value.ShouldBeOfType<CheckoutStockConflictResponseDto>();
        response.Errors.Count.ShouldBe(1);
        response.Errors[0].AvailableQuantity.ShouldBe(1);
    }

    [Fact]
    public async Task PlaceOrder_ReturnsBadRequestWhenServiceRejectsCheckout()
    {
        var userId = Guid.NewGuid();
        var request = CreateRequest();
        var checkoutService = new Mock<ICheckoutService>();
        var controller = new CheckoutController(checkoutService.Object)
        {
            ControllerContext = ControllerTestHelper.CreateControllerContext(userId)
        };

        checkoutService
            .Setup(service => service.PlaceOrderAsync(
                userId,
                request,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Checkout must contain at least one item."));

        var result = await controller.PlaceOrder(request, CancellationToken.None);

        var badRequestResult = result.Result.ShouldBeOfType<BadRequestObjectResult>();
        var error = badRequestResult.Value.ShouldBeOfType<ApiErrorResponseDto>();
        error.Message.ShouldBe("Checkout must contain at least one item.");
    }

    private static CheckoutRequestDto CreateRequest()
    {
        return new CheckoutRequestDto
        {
            Items =
            [
                new CheckoutItemRequestDto
                {
                    ProductId = Guid.NewGuid(),
                    Size = ProductSize.M,
                    Quantity = 2
                }
            ],
            RecipientName = "Adrian Chiriac",
            AddressLine = "123 Main Street",
            City = "Bucharest",
            PostalCode = "010101",
            Country = "Romania"
        };
    }
}
