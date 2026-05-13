using Ecommerce.Api.DTOs.Products;
using Ecommerce.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService productService;

    public ProductsController(IProductService productService)
    {
        this.productService = productService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ProductsResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductsResponseDto>> GetAll(CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(cancellationToken);

        return Ok(products);
    }
}
