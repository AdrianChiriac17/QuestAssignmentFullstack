using Ecommerce.Api.Models;
using Ecommerce.Api.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Ecommerce.Api.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly string connectionString;

    public ProductRepository(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Name, Description, Price, FrontImageUrl, BackImageUrl, CreatedAt, UpdatedAt
            FROM Products
            ORDER BY Name;
            """;

        var products = new List<Product>();

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(MapProduct(reader));
        }

        return products;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Name, Description, Price, FrontImageUrl, BackImageUrl, CreatedAt, UpdatedAt
            FROM Products
            WHERE Id = @Id;
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapProduct(reader) : null;
    }

    public async Task<IReadOnlyList<ProductSizeStock>> GetSizeStocksByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, ProductId, Size, StockQuantity, UpdatedAt
            FROM ProductSizeStocks
            WHERE ProductId = @ProductId
            ORDER BY
                CASE Size
                    WHEN N'XS' THEN 1
                    WHEN N'S' THEN 2
                    WHEN N'M' THEN 3
                    WHEN N'L' THEN 4
                    WHEN N'XL' THEN 5
                    WHEN N'XXL' THEN 6
                    ELSE 7
                END;
            """;

        var sizeStocks = new List<ProductSizeStock>();

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ProductId", productId);

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            sizeStocks.Add(MapProductSizeStock(reader));
        }

        return sizeStocks;
    }

    public async Task<IReadOnlyList<ProductSizeStock>> GetSizeStocksByProductIdsAsync(
        IReadOnlyList<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        var uniqueProductIds = productIds.Distinct().ToList();

        if (uniqueProductIds.Count == 0)
        {
            return [];
        }

        var parameterNames = uniqueProductIds
            .Select((_, index) => $"@ProductId{index}")
            .ToList();

        var sql = $"""
            SELECT Id, ProductId, Size, StockQuantity, UpdatedAt
            FROM ProductSizeStocks
            WHERE ProductId IN ({string.Join(", ", parameterNames)})
            ORDER BY ProductId,
                CASE Size
                    WHEN N'XS' THEN 1
                    WHEN N'S' THEN 2
                    WHEN N'M' THEN 3
                    WHEN N'L' THEN 4
                    WHEN N'XL' THEN 5
                    WHEN N'XXL' THEN 6
                    ELSE 7
                END;
            """;

        var sizeStocks = new List<ProductSizeStock>();

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);

        for (var index = 0; index < uniqueProductIds.Count; index++)
        {
            command.Parameters.AddWithValue(parameterNames[index], uniqueProductIds[index]);
        }

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            sizeStocks.Add(MapProductSizeStock(reader));
        }

        return sizeStocks;
    }

    public async Task<int> GetAvailableStockAsync(
        Guid productId,
        ProductSize size,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT StockQuantity
            FROM ProductSizeStocks
            WHERE ProductId = @ProductId
              AND Size = @Size;
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ProductId", productId);
        command.Parameters.AddWithValue("@Size", size.ToString());

        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result is null ? 0 : Convert.ToInt32(result);
    }

    public async Task DecreaseStockAsync(
        Guid productId,
        ProductSize size,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        const string sql = """
            UPDATE ProductSizeStocks
            SET StockQuantity = StockQuantity - @Quantity,
                UpdatedAt = SYSDATETIMEOFFSET()
            WHERE ProductId = @ProductId
              AND Size = @Size
              AND StockQuantity >= @Quantity;
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ProductId", productId);
        command.Parameters.AddWithValue("@Size", size.ToString());
        command.Parameters.AddWithValue("@Quantity", quantity);

        await connection.OpenAsync(cancellationToken);
        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

        if (affectedRows == 0)
        {
            throw new InvalidOperationException("Stock could not be decreased because the product size was not found or stock was insufficient.");
        }
    }

    private static Product MapProduct(SqlDataReader reader)
    {
        var backImageUrlOrdinal = reader.GetOrdinal("BackImageUrl");

        return new Product
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Description = reader.GetString(reader.GetOrdinal("Description")),
            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
            FrontImageUrl = reader.GetString(reader.GetOrdinal("FrontImageUrl")),
            BackImageUrl = reader.IsDBNull(backImageUrlOrdinal)
                ? null
                : reader.GetString(backImageUrlOrdinal),
            CreatedAt = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("UpdatedAt"))
        };
    }

    private static ProductSizeStock MapProductSizeStock(SqlDataReader reader)
    {
        return new ProductSizeStock
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            ProductId = reader.GetGuid(reader.GetOrdinal("ProductId")),
            Size = Enum.Parse<ProductSize>(reader.GetString(reader.GetOrdinal("Size"))),
            StockQuantity = reader.GetInt32(reader.GetOrdinal("StockQuantity")),
            UpdatedAt = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("UpdatedAt"))
        };
    }
}
