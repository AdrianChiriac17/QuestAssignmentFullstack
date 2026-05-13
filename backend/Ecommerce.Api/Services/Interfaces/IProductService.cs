using Ecommerce.Api.DTOs.Products;

namespace Ecommerce.Api.Services.Interfaces;

public interface IProductService
{
    Task<ProductsResponseDto> GetAllAsync(CancellationToken cancellationToken = default);
}
