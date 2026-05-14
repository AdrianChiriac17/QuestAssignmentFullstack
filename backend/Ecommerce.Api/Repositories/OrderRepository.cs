using Ecommerce.Api.Models;
using Ecommerce.Api.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Ecommerce.Api.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly string connectionString;

    public OrderRepository(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");
    }

    public async Task CreateWithItemsAndDecreaseStockAsync(
        Order order,
        IReadOnlyList<OrderItem> orderItems,
        CancellationToken cancellationToken = default)
    {
        if (orderItems.Count == 0)
        {
            throw new ArgumentException("An order must contain at least one item.", nameof(orderItems));
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await InsertOrderAsync(connection, transaction, order, cancellationToken);

            foreach (var orderItem in orderItems)
            {
                await InsertOrderItemAsync(connection, transaction, orderItem, cancellationToken);
                await DecreaseStockAsync(connection, transaction, orderItem, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, UserId, RecipientName, AddressLine, City, PostalCode, Country, TotalPrice, CreatedAt
            FROM Orders
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC;
            """;

        var orders = new List<Order>();

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            orders.Add(MapOrder(reader));
        }

        return orders;
    }

    public async Task<Order?> GetByIdForUserAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, UserId, RecipientName, AddressLine, City, PostalCode, Country, TotalPrice, CreatedAt
            FROM Orders
            WHERE Id = @Id
              AND UserId = @UserId;
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", orderId);
        command.Parameters.AddWithValue("@UserId", userId);

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapOrder(reader) : null;
    }

    public async Task<IReadOnlyList<OrderItem>> GetItemsByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, OrderId, ProductId, ProductName, Size, UnitPrice, Quantity
            FROM OrderItems
            WHERE OrderId = @OrderId
            ORDER BY ProductName, Size;
            """;

        var orderItems = new List<OrderItem>();

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@OrderId", orderId);

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            orderItems.Add(MapOrderItem(reader));
        }

        return orderItems;
    }

    private static async Task InsertOrderAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        Order order,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO Orders
                (Id, UserId, RecipientName, AddressLine, City, PostalCode, Country, TotalPrice, CreatedAt)
            VALUES
                (@Id, @UserId, @RecipientName, @AddressLine, @City, @PostalCode, @Country, @TotalPrice, @CreatedAt);
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@Id", order.Id);
        command.Parameters.AddWithValue("@UserId", order.UserId);
        command.Parameters.AddWithValue("@RecipientName", order.RecipientName);
        command.Parameters.AddWithValue("@AddressLine", order.AddressLine);
        command.Parameters.AddWithValue("@City", order.City);
        command.Parameters.AddWithValue("@PostalCode", order.PostalCode);
        command.Parameters.AddWithValue("@Country", order.Country);
        command.Parameters.AddWithValue("@TotalPrice", order.TotalPrice);
        command.Parameters.AddWithValue("@CreatedAt", order.CreatedAt);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertOrderItemAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        OrderItem orderItem,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO OrderItems
                (Id, OrderId, ProductId, ProductName, Size, UnitPrice, Quantity)
            VALUES
                (@Id, @OrderId, @ProductId, @ProductName, @Size, @UnitPrice, @Quantity);
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@Id", orderItem.Id);
        command.Parameters.AddWithValue("@OrderId", orderItem.OrderId);
        command.Parameters.AddWithValue("@ProductId", orderItem.ProductId);
        command.Parameters.AddWithValue("@ProductName", orderItem.ProductName);
        command.Parameters.AddWithValue("@Size", orderItem.Size.ToString());
        command.Parameters.AddWithValue("@UnitPrice", orderItem.UnitPrice);
        command.Parameters.AddWithValue("@Quantity", orderItem.Quantity);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task DecreaseStockAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        OrderItem orderItem,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE ProductSizeStocks
            SET StockQuantity = StockQuantity - @Quantity,
                UpdatedAt = SYSDATETIMEOFFSET()
            WHERE ProductId = @ProductId
              AND Size = @Size
              AND StockQuantity >= @Quantity;
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@ProductId", orderItem.ProductId);
        command.Parameters.AddWithValue("@Size", orderItem.Size.ToString());
        command.Parameters.AddWithValue("@Quantity", orderItem.Quantity);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affectedRows == 0)
        {
            throw new InvalidOperationException(
                "Stock could not be decreased because the product size was not found or stock was insufficient.");
        }
    }

    private static Order MapOrder(SqlDataReader reader)
    {
        return new Order
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
            RecipientName = reader.GetString(reader.GetOrdinal("RecipientName")),
            AddressLine = reader.GetString(reader.GetOrdinal("AddressLine")),
            City = reader.GetString(reader.GetOrdinal("City")),
            PostalCode = reader.GetString(reader.GetOrdinal("PostalCode")),
            Country = reader.GetString(reader.GetOrdinal("Country")),
            TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
            CreatedAt = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("CreatedAt"))
        };
    }

    private static OrderItem MapOrderItem(SqlDataReader reader)
    {
        return new OrderItem
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            OrderId = reader.GetGuid(reader.GetOrdinal("OrderId")),
            ProductId = reader.GetGuid(reader.GetOrdinal("ProductId")),
            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
            Size = Enum.Parse<ProductSize>(reader.GetString(reader.GetOrdinal("Size"))),
            UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
        };
    }
}
