using Ecommerce.Api.Models;

namespace Ecommerce.Api.Repositories.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductSizeStock>> GetSizeStocksByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductSizeStock>> GetSizeStocksByProductIdsAsync(
        IReadOnlyList<Guid> productIds,
        CancellationToken cancellationToken = default);

    Task<int> GetAvailableStockAsync(
        Guid productId,
        ProductSize size,
        CancellationToken cancellationToken = default);

    Task DecreaseStockAsync(
        Guid productId,
        ProductSize size,
        int quantity,
        CancellationToken cancellationToken = default);
}
