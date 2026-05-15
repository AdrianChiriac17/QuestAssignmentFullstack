using Ecommerce.Api.Models;
using Ecommerce.Api.Repositories.Interfaces;
using Ecommerce.Api.Services;
using Moq;
using Shouldly;

namespace Ecommerce.Api.Tests.Services;

public class OrderServiceTests
{
    [Fact]
    public async Task GetByUserIdAsync_MapsOrdersToSummaryResponse()
    {
        var userId = Guid.NewGuid();
        var orders = new List<Order>
        {
            CreateOrder(userId, 119.98m),
            CreateOrder(userId, 59.99m)
        };
        var orderRepository = new Mock<IOrderRepository>();
        var service = new OrderService(orderRepository.Object);

        orderRepository
            .Setup(repository => repository.GetByUserIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var response = await service.GetByUserIdAsync(userId);

        response.Orders.Count.ShouldBe(2);
        response.Orders[0].Id.ShouldBe(orders[0].Id);
        response.Orders[0].TotalPrice.ShouldBe(orders[0].TotalPrice);
        response.Orders[0].CreatedAt.ShouldBe(orders[0].CreatedAt);
    }

    [Fact]
    public async Task GetByIdForUserAsync_ReturnsNullWhenOrderDoesNotExistForUser()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var service = new OrderService(orderRepository.Object);

        orderRepository
            .Setup(repository => repository.GetByIdForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var response = await service.GetByIdForUserAsync(Guid.NewGuid(), Guid.NewGuid());

        response.ShouldBeNull();
        orderRepository.Verify(
            repository => repository.GetItemsByOrderIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdForUserAsync_LoadsItemsAndMapsDetailResponseWhenOrderExists()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId, 129.98m);
        var items = new List<OrderItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = Guid.NewGuid(),
                ProductName = "Crimson Home Shirt",
                Size = ProductSize.M,
                UnitPrice = 59.99m,
                Quantity = 1
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = Guid.NewGuid(),
                ProductName = "Obsidian Training Shirt",
                Size = ProductSize.L,
                UnitPrice = 69.99m,
                Quantity = 1
            }
        };
        var orderRepository = new Mock<IOrderRepository>();
        var service = new OrderService(orderRepository.Object);

        orderRepository
            .Setup(repository => repository.GetByIdForUserAsync(
                order.Id,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        orderRepository
            .Setup(repository => repository.GetItemsByOrderIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var response = await service.GetByIdForUserAsync(order.Id, userId);

        response.ShouldNotBeNull();
        response.Id.ShouldBe(order.Id);
        response.TotalPrice.ShouldBe(order.TotalPrice);
        response.RecipientName.ShouldBe(order.RecipientName);
        response.Items.Count.ShouldBe(2);
        response.Items[0].ProductName.ShouldBe(items[0].ProductName);
        response.Items[0].LineTotal.ShouldBe(items[0].LineTotal);
    }

    private static Order CreateOrder(Guid userId, decimal totalPrice)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RecipientName = "Adrian Chiriac",
            AddressLine = "123 Main Street",
            City = "Bucharest",
            PostalCode = "010101",
            Country = "Romania",
            TotalPrice = totalPrice,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
