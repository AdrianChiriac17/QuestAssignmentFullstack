namespace Ecommerce.Api.Models;

public class ProductSizeStock
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public ProductSize Size { get; set; }

    public int StockQuantity { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
