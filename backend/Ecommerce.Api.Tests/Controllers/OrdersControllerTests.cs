using Ecommerce.Api.Controllers;
using Ecommerce.Api.DTOs.Errors;
using Ecommerce.Api.DTOs.Orders;
using Ecommerce.Api.Models;
using Ecommerce.Api.Services.Interfaces;
using Ecommerce.Api.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace Ecommerce.Api.Tests.Controllers;

public class OrdersControllerTests
{
    [Fact]
    public async Task GetOrders_ReturnsOrdersForAuthenticatedUser()
    {
        var userId = Guid.NewGuid();
        var response = new OrdersResponseDto(
            [new OrderSummaryResponseDto(Guid.NewGuid(), 119.98m, DateTimeOffset.UtcNow)]);
        var orderService = new Mock<IOrderService>();
        var controller = new OrdersController(orderService.Object)
        {
            ControllerContext = ControllerTestHelper.CreateControllerContext(userId)
        };

        orderService
            .Setup(service => service.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await controller.GetOrders(CancellationToken.None);

        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(response);
    }

    [Fact]
    public async Task GetOrders_ReturnsUnauthorizedWhenUserIdClaimIsMissing()
    {
        var orderService = new Mock<IOrderService>();
        var controller = new OrdersController(orderService.Object)
        {
            ControllerContext = ControllerTestHelper.CreateControllerContextWithoutUserId()
        };

        var result = await controller.GetOrders(CancellationToken.None);

        var unauthorizedResult = result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
        var error = unauthorizedResult.Value.ShouldBeOfType<ApiErrorResponseDto>();
        error.Message.ShouldBe("Invalid authentication token.");
        orderService.Verify(
            service => service.GetByUserIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetOrder_ReturnsOrderDetailWhenOwnedOrderExists()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var response = CreateOrderResponse(orderId);
        var orderService = new Mock<IOrderService>();
        var controller = new OrdersController(orderService.Object)
        {
            ControllerContext = ControllerTestHelper.CreateControllerContext(userId)
        };

        orderService
            .Setup(service => service.GetByIdForUserAsync(
                orderId,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await controller.GetOrder(orderId, CancellationToken.None);

        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(response);
    }

    [Fact]
    public async Task GetOrder_ReturnsNotFoundWhenOrderDoesNotExistForUser()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var orderService = new Mock<IOrderService>();
        var controller = new OrdersController(orderService.Object)
        {
            ControllerContext = ControllerTestHelper.CreateControllerContext(userId)
        };

        orderService
            .Setup(service => service.GetByIdForUserAsync(
                orderId,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderResponseDto?)null);

        var result = await controller.GetOrder(orderId, CancellationToken.None);

        result.Result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetOrder_ReturnsUnauthorizedWhenUserIdClaimIsMissing()
    {
        var orderService = new Mock<IOrderService>();
        var controller = new OrdersController(orderService.Object)
        {
            ControllerContext = ControllerTestHelper.CreateControllerContextWithoutUserId()
        };

        var result = await controller.GetOrder(Guid.NewGuid(), CancellationToken.None);

        var unauthorizedResult = result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
        var error = unauthorizedResult.Value.ShouldBeOfType<ApiErrorResponseDto>();
        error.Message.ShouldBe("Invalid authentication token.");
    }

    private static OrderResponseDto CreateOrderResponse(Guid orderId)
    {
        return new OrderResponseDto(
            orderId,
            "Adrian Chiriac",
            "123 Main Street",
            "Bucharest",
            "010101",
            "Romania",
            59.99m,
            DateTimeOffset.UtcNow,
            [
                new OrderItemResponseDto(
                    Guid.NewGuid(),
                    "Crimson Home Shirt",
                    ProductSize.M,
                    59.99m,
                    1,
                    59.99m)
            ]);
    }
}
