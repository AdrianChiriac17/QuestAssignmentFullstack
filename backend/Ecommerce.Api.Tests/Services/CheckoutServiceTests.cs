using Ecommerce.Api.DTOs.Checkout;
using Ecommerce.Api.Models;
using Ecommerce.Api.Repositories.Interfaces;
using Ecommerce.Api.Services;
using Ecommerce.Api.Services.Exceptions;
using Moq;
using Shouldly;

namespace Ecommerce.Api.Tests.Services;

public class CheckoutServiceTests
{
    [Fact]
    public async Task PlaceOrderAsync_CalculatesTotalFromRepositoryProducts()
    {
        var product = CreateProduct(price: 59.99m);
        var productRepository = new Mock<IProductRepository>();
        var orderRepository = new Mock<IOrderRepository>();
        Order? capturedOrder = null;
        IReadOnlyList<OrderItem>? capturedItems = null;

        productRepository
            .Setup(repository => repository.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        productRepository
            .Setup(repository => repository.GetSizeStocksByProductIdsAsync(
                It.IsAny<IReadOnlyList<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateStock(product.Id, ProductSize.M, 5)]);
        orderRepository
            .Setup(repository => repository.CreateWithItemsAndDecreaseStockAsync(
                It.IsAny<Order>(),
                It.IsAny<IReadOnlyList<OrderItem>>(),
                It.IsAny<CancellationToken>()))
            .Callback<Order, IReadOnlyList<OrderItem>, CancellationToken>((order, items, _) =>
            {
                capturedOrder = order;
                capturedItems = items;
            })
            .Returns(Task.CompletedTask);

        var service = new CheckoutService(productRepository.Object, orderRepository.Object);

        var response = await service.PlaceOrderAsync(
            Guid.NewGuid(),
            CreateRequest(product.Id, ProductSize.M, 2));

        response.TotalPrice.ShouldBe(119.98m);
        capturedOrder.ShouldNotBeNull();
        capturedOrder.TotalPrice.ShouldBe(119.98m);
        capturedItems.ShouldNotBeNull();
        capturedItems.Count.ShouldBe(1);
        capturedItems[0].ProductId.ShouldBe(product.Id);
        capturedItems[0].ProductName.ShouldBe(product.Name);
        capturedItems[0].UnitPrice.ShouldBe(product.Price);
        capturedItems[0].Quantity.ShouldBe(2);
    }

    [Fact]
    public async Task PlaceOrderAsync_MergesDuplicateProductSizeLinesBeforeCreatingOrder()
    {
        var product = CreateProduct(price: 10m);
        var productRepository = new Mock<IProductRepository>();
        var orderRepository = new Mock<IOrderRepository>();
        IReadOnlyList<OrderItem>? capturedItems = null;

        productRepository
            .Setup(repository => repository.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        productRepository
            .Setup(repository => repository.GetSizeStocksByProductIdsAsync(
                It.IsAny<IReadOnlyList<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateStock(product.Id, ProductSize.L, 5)]);
        orderRepository
            .Setup(repository => repository.CreateWithItemsAndDecreaseStockAsync(
                It.IsAny<Order>(),
                It.IsAny<IReadOnlyList<OrderItem>>(),
                It.IsAny<CancellationToken>()))
            .Callback<Order, IReadOnlyList<OrderItem>, CancellationToken>((_, items, _) =>
            {
                capturedItems = items;
            })
            .Returns(Task.CompletedTask);

        var service = new CheckoutService(productRepository.Object, orderRepository.Object);
        var request = CreateRequest(product.Id, ProductSize.L, 1);
        request.Items.Add(new CheckoutItemRequestDto
        {
            ProductId = product.Id,
            Size = ProductSize.L,
            Quantity = 2
        });

        var response = await service.PlaceOrderAsync(Guid.NewGuid(), request);

        response.TotalPrice.ShouldBe(30m);
        capturedItems.ShouldNotBeNull();
        capturedItems.Count.ShouldBe(1);
        capturedItems[0].Quantity.ShouldBe(3);
    }

    [Fact]
    public async Task PlaceOrderAsync_ThrowsStockConflictWhenRequestedQuantityExceedsAvailableStock()
    {
        var product = CreateProduct(price: 49.99m);
        var productRepository = new Mock<IProductRepository>();
        var orderRepository = new Mock<IOrderRepository>();

        productRepository
            .Setup(repository => repository.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        productRepository
            .Setup(repository => repository.GetSizeStocksByProductIdsAsync(
                It.IsAny<IReadOnlyList<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateStock(product.Id, ProductSize.XL, 1)]);

        var service = new CheckoutService(productRepository.Object, orderRepository.Object);

        var exception = await Should.ThrowAsync<CheckoutStockConflictException>(
            () => service.PlaceOrderAsync(Guid.NewGuid(), CreateRequest(product.Id, ProductSize.XL, 2)));

        exception.Response.Errors.Count.ShouldBe(1);
        exception.Response.Errors[0].ProductId.ShouldBe(product.Id);
        exception.Response.Errors[0].RequestedQuantity.ShouldBe(2);
        exception.Response.Errors[0].AvailableQuantity.ShouldBe(1);
        orderRepository.Verify(
            repository => repository.CreateWithItemsAndDecreaseStockAsync(
                It.IsAny<Order>(),
                It.IsAny<IReadOnlyList<OrderItem>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PlaceOrderAsync_ConvertsRepositoryStockFailureIntoStockConflict()
    {
        var product = CreateProduct(price: 49.99m);
        var productRepository = new Mock<IProductRepository>();
        var orderRepository = new Mock<IOrderRepository>();

        productRepository
            .Setup(repository => repository.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        productRepository
            .SetupSequence(repository => repository.GetSizeStocksByProductIdsAsync(
                It.IsAny<IReadOnlyList<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateStock(product.Id, ProductSize.S, 3)])
            .ReturnsAsync([CreateStock(product.Id, ProductSize.S, 1)]);
        orderRepository
            .Setup(repository => repository.CreateWithItemsAndDecreaseStockAsync(
                It.IsAny<Order>(),
                It.IsAny<IReadOnlyList<OrderItem>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Stock changed."));

        var service = new CheckoutService(productRepository.Object, orderRepository.Object);

        var exception = await Should.ThrowAsync<CheckoutStockConflictException>(
            () => service.PlaceOrderAsync(Guid.NewGuid(), CreateRequest(product.Id, ProductSize.S, 2)));

        exception.Response.Errors.Count.ShouldBe(1);
        exception.Response.Errors[0].AvailableQuantity.ShouldBe(1);
    }

    private static CheckoutRequestDto CreateRequest(
        Guid productId,
        ProductSize size,
        int quantity)
    {
        return new CheckoutRequestDto
        {
            Items =
            [
                new CheckoutItemRequestDto
                {
                    ProductId = productId,
                    Size = size,
                    Quantity = quantity
                }
            ],
            RecipientName = "Adrian Chiriac",
            AddressLine = "123 Main Street",
            City = "Bucharest",
            PostalCode = "010101",
            Country = "Romania"
        };
    }

    private static Product CreateProduct(decimal price)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Shirt",
            Description = "Test product",
            Price = price,
            FrontImageUrl = "/images/products/test.jpg",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    private static ProductSizeStock CreateStock(
        Guid productId,
        ProductSize size,
        int quantity)
    {
        return new ProductSizeStock
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Size = size,
            StockQuantity = quantity,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
