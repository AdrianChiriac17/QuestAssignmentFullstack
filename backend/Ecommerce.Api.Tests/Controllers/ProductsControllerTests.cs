using Ecommerce.Api.Controllers;
using Ecommerce.Api.DTOs.Products;
using Ecommerce.Api.Models;
using Ecommerce.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace Ecommerce.Api.Tests.Controllers;

public class ProductsControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsProductsFromService()
    {
        var response = new ProductsResponseDto(
            [
                new ProductResponseDto(
                    Guid.NewGuid(),
                    "Crimson Home Shirt",
                    "A clean crimson football shirt.",
                    59.99m,
                    "/images/products/product.jpg",
                    "/images/products/product-back.jpg",
                    [new ProductSizeStockResponseDto(ProductSize.M, 5)])
            ]);
        var productService = new Mock<IProductService>();
        var controller = new ProductsController(productService.Object);

        productService
            .Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await controller.GetAll(CancellationToken.None);

        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(response);
    }
}
