using Ecommerce.Api.Models;
using Ecommerce.Api.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Ecommerce.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string connectionString;

    public UserRepository(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Email, PasswordHash, FirstName, LastName, CreatedAt
            FROM Users
            WHERE Id = @Id;
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapUser(reader) : null;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Email, PasswordHash, FirstName, LastName, CreatedAt
            FROM Users
            WHERE Email = @Email;
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapUser(reader) : null;
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 1
            FROM Users
            WHERE Email = @Email;
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);

        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result is not null;
    }

    public async Task CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, CreatedAt)
            VALUES (@Id, @Email, @PasswordHash, @FirstName, @LastName, @CreatedAt);
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", user.Id);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        command.Parameters.AddWithValue("@FirstName", user.FirstName);
        command.Parameters.AddWithValue("@LastName", user.LastName);
        command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            DELETE FROM Users
            WHERE Id = @Id;
            """;

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static User MapUser(SqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            CreatedAt = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("CreatedAt"))
        };
    }
}
