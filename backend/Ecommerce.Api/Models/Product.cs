namespace Ecommerce.Api.Models;

public class Product
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public decimal Price { get; set; }

    public required string FrontImageUrl { get; set; }

    public string? BackImageUrl { get; set; }

    public int StockQuantity { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
