using Ecommerce.Api.Helpers;
using Microsoft.Data.SqlClient;

namespace Ecommerce.Api.Data;

public static class SeedData
{
    private static readonly Guid DemoUserId = Guid.Parse("9ceaa99f-50d7-4ac1-bd5e-e30a9ea7733c");
    private static readonly Guid ReviewerUserId = Guid.Parse("f1f377c0-4235-4aaf-8ae5-b5ca6a7fa602");

    private static readonly ProductSeed[] Products =
    [
        new(
            Guid.Parse("4e892b3f-8707-44c6-a9ec-84e6af3764ad"),
            "Crimson Home Shirt",
            "A clean crimson football shirt with white trim for match day.",
            59.99m,
            StockBySize(8, 12, 15, 10, 6, 2)),
        new(
            Guid.Parse("c93a8f32-c49b-4a09-9c08-8ea44c0d6ac8"),
            "Midnight Away Shirt",
            "A deep navy away shirt with silver details and a lightweight feel.",
            64.99m,
            StockBySize(3, 7, 11, 9, 0, 1)),
        new(
            Guid.Parse("ac3ce776-806e-4b3e-b98d-34c8db5c2a5c"),
            "Emerald Goalkeeper Shirt",
            "A bold green goalkeeper shirt with long-sleeve inspired styling.",
            69.99m,
            StockBySize(0, 4, 6, 5, 3, 0)),
        new(
            Guid.Parse("1fab1d9f-ef39-4dae-b2f0-06c8bfbc4bf4"),
            "Obsidian Training Shirt",
            "A black training shirt with gold accents for everyday sessions.",
            49.99m,
            StockBySize(10, 14, 18, 13, 7, 4))
    ];

    public static async Task InitializeAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await SeedUsersAsync(connection, passwordHasher);
        await SeedProductsAsync(connection);
    }

    private static async Task SeedUsersAsync(SqlConnection connection, IPasswordHasher passwordHasher)
    {
        await InsertUserIfMissingAsync(
            connection,
            DemoUserId,
            "demo@questshirts.local",
            passwordHasher.HashPassword("Password123!"),
            "Demo",
            "User");

        await InsertUserIfMissingAsync(
            connection,
            ReviewerUserId,
            "reviewer@questshirts.local",
            passwordHasher.HashPassword("Password123!"),
            "Quest",
            "Reviewer");
    }

    private static async Task InsertUserIfMissingAsync(
        SqlConnection connection,
        Guid id,
        string email,
        string passwordHash,
        string firstName,
        string lastName)
    {
        const string sql = """
            IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
            BEGIN
                INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, CreatedAt)
                VALUES (@Id, @Email, @PasswordHash, @FirstName, @LastName, @CreatedAt);
            END;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@PasswordHash", passwordHash);
        command.Parameters.AddWithValue("@FirstName", firstName);
        command.Parameters.AddWithValue("@LastName", lastName);
        command.Parameters.AddWithValue("@CreatedAt", DateTimeOffset.UtcNow);

        await command.ExecuteNonQueryAsync();
    }

    private static async Task SeedProductsAsync(SqlConnection connection)
    {
        foreach (var product in Products)
        {
            await InsertProductIfMissingAsync(connection, product);

            foreach (var stock in product.Stocks)
            {
                await InsertProductSizeStockIfMissingAsync(connection, product.Id, stock);
            }
        }
    }

    private static async Task InsertProductIfMissingAsync(SqlConnection connection, ProductSeed product)
    {
        const string sql = """
            IF NOT EXISTS (SELECT 1 FROM Products WHERE Id = @Id)
            BEGIN
                INSERT INTO Products
                    (Id, Name, Description, Price, FrontImageUrl, BackImageUrl, CreatedAt, UpdatedAt)
                VALUES
                    (@Id, @Name, @Description, @Price, @FrontImageUrl, @BackImageUrl, @CreatedAt, @UpdatedAt);
            END;
            """;

        var now = DateTimeOffset.UtcNow;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", product.Id);
        command.Parameters.AddWithValue("@Name", product.Name);
        command.Parameters.AddWithValue("@Description", product.Description);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@FrontImageUrl", $"/images/products/{product.Id}.jpg");
        command.Parameters.AddWithValue("@BackImageUrl", $"/images/products/{product.Id}-back.jpg");
        command.Parameters.AddWithValue("@CreatedAt", now);
        command.Parameters.AddWithValue("@UpdatedAt", now);

        await command.ExecuteNonQueryAsync();
    }

    private static async Task InsertProductSizeStockIfMissingAsync(
        SqlConnection connection,
        Guid productId,
        ProductSizeStockSeed stock)
    {
        const string sql = """
            IF NOT EXISTS (
                SELECT 1
                FROM ProductSizeStocks
                WHERE ProductId = @ProductId
                  AND Size = @Size)
            BEGIN
                INSERT INTO ProductSizeStocks (Id, ProductId, Size, StockQuantity, UpdatedAt)
                VALUES (@Id, @ProductId, @Size, @StockQuantity, @UpdatedAt);
            END;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", Guid.NewGuid());
        command.Parameters.AddWithValue("@ProductId", productId);
        command.Parameters.AddWithValue("@Size", stock.Size);
        command.Parameters.AddWithValue("@StockQuantity", stock.Quantity);
        command.Parameters.AddWithValue("@UpdatedAt", DateTimeOffset.UtcNow);

        await command.ExecuteNonQueryAsync();
    }

    private static IReadOnlyList<ProductSizeStockSeed> StockBySize(
        int xs,
        int s,
        int m,
        int l,
        int xl,
        int xxl)
    {
        return
        [
            new("XS", xs),
            new("S", s),
            new("M", m),
            new("L", l),
            new("XL", xl),
            new("XXL", xxl)
        ];
    }

    private sealed record ProductSeed(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        IReadOnlyList<ProductSizeStockSeed> Stocks);

    private sealed record ProductSizeStockSeed(string Size, int Quantity);
}
