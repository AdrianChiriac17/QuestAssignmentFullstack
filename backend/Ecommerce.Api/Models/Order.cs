namespace Ecommerce.Api.Models;

public class Order
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string RecipientName { get; set; }

    public required string AddressLine { get; set; }

    public required string City { get; set; }

    public required string PostalCode { get; set; }

    public required string Country { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
