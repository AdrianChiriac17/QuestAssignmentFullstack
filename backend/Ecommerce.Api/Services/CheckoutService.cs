using Ecommerce.Api.DTOs.Checkout;
using Ecommerce.Api.DTOs.Errors;
using Ecommerce.Api.Mappings;
using Ecommerce.Api.Models;
using Ecommerce.Api.Repositories.Interfaces;
using Ecommerce.Api.Services.Exceptions;
using Ecommerce.Api.Services.Interfaces;

namespace Ecommerce.Api.Services;

public class CheckoutService : ICheckoutService
{
    private readonly IProductRepository productRepository;
    private readonly IOrderRepository orderRepository;

    public CheckoutService(
        IProductRepository productRepository,
        IOrderRepository orderRepository)
    {
        this.productRepository = productRepository;
        this.orderRepository = orderRepository;
    }

    public async Task<CheckoutResponseDto> PlaceOrderAsync(
        Guid userId,
        CheckoutRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var normalizedRequest = NormalizeRequest(request);
        if (normalizedRequest.Items.Count == 0)
        {
            throw new InvalidOperationException("Checkout must contain at least one item.");
        }

        var productIdsFromRequest = normalizedRequest.Items.Select(item => item.ProductId).Distinct().ToList();

        var productsByIdDictionary = await GetProductsByIdAsync(productIdsFromRequest, cancellationToken);

        var conflicts = await GetStockConflictsAsync(
            normalizedRequest.Items,
            productsByIdDictionary,
            cancellationToken);

        if (conflicts.Count > 0)
        {
            throw new CheckoutStockConflictException(conflicts);
        }

        decimal totalPrice = normalizedRequest.Items.Sum(
            item => productsByIdDictionary[item.ProductId].Price * item.Quantity);

        var order = normalizedRequest.ToOrder(userId, totalPrice);
        var orderItems = normalizedRequest.Items
            .Select(item => ToOrderItem(item, order.Id, productsByIdDictionary[item.ProductId]))
            .ToList();

        try
        {
            await orderRepository.CreateWithItemsAndDecreaseStockAsync(
                order,
                orderItems,
                cancellationToken);
        }
        catch (InvalidOperationException)
        {
            var latestConflicts = await GetStockConflictsAsync(
                normalizedRequest.Items,
                productsByIdDictionary,
                cancellationToken);

            if (latestConflicts.Count > 0)
            {
                throw new CheckoutStockConflictException(latestConflicts);
            }

            throw;
        }

        return order.ToCheckoutResponseDto();
    }

    private async Task<Dictionary<Guid, Product>> GetProductsByIdAsync(
        IReadOnlyList<Guid> productIds,
        CancellationToken cancellationToken)
    {
        var productsById = new Dictionary<Guid, Product>();

        foreach (var productId in productIds)
        {
            var product = await productRepository.GetByIdAsync(productId, cancellationToken);
            if (product is not null)
            {
                productsById[product.Id] = product;
            }
        }

        return productsById;
    }

    private async Task<IReadOnlyList<CheckoutStockConflictItemResponseDto>> GetStockConflictsAsync(
        IReadOnlyList<CheckoutItemRequestDto> items,
        IReadOnlyDictionary<Guid, Product> productsById,
        CancellationToken cancellationToken)
    {
        var productIdsList = items.Select(item => item.ProductId).Distinct().ToList();
        var productSizeStocksList = await productRepository.GetSizeStocksByProductIdsAsync(
            productIdsList,
            cancellationToken);

        var stockByProductAndSizeDictionary = productSizeStocksList.ToDictionary(
            sizeStock => (sizeStock.ProductId, sizeStock.Size),
            sizeStock => sizeStock.StockQuantity);

        var conflicts = new List<CheckoutStockConflictItemResponseDto>();

        foreach (var item in items)
        {
            var availableQuantity = 0;
            var productExists = productsById.ContainsKey(item.ProductId);
            if (productExists &&
                stockByProductAndSizeDictionary.TryGetValue(
                    (item.ProductId, item.Size),
                    out var stockQuantity))
            {
                availableQuantity = stockQuantity;
            }

            if (item.Quantity <= availableQuantity)
            {
                continue;
            }

            var conflict = item.ToCheckoutStockConflictItemResponseDto(availableQuantity);

            conflicts.Add(conflict);
        }

        return conflicts;
    }

    private static CheckoutRequestDto NormalizeRequest(CheckoutRequestDto request)
    {
        return new CheckoutRequestDto
        {
            Items = request.Items
                .GroupBy(item => new { item.ProductId, item.Size })
                .Select(group => new CheckoutItemRequestDto
                {
                    ProductId = group.Key.ProductId,
                    Size = group.Key.Size,
                    Quantity = group.Sum(item => item.Quantity)
                })
                .Where(item => item.Quantity > 0)
                .ToList(),
            RecipientName = request.RecipientName.Trim(),
            AddressLine = request.AddressLine.Trim(),
            City = request.City.Trim(),
            PostalCode = request.PostalCode.Trim(),
            Country = request.Country.Trim()
        };
    }

    private static OrderItem ToOrderItem(
        CheckoutItemRequestDto item,
        Guid orderId,
        Product product)
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = product.Id,
            ProductName = product.Name,
            Size = item.Size,
            UnitPrice = product.Price,
            Quantity = item.Quantity
        };
    }

}
