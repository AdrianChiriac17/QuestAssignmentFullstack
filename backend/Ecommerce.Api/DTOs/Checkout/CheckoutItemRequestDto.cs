using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Checkout;

public class CheckoutItemRequestDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; }
}
