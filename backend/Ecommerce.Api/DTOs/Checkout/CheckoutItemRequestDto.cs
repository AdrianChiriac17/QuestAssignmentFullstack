using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.Models;

namespace Ecommerce.Api.DTOs.Checkout;

public class CheckoutItemRequestDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public ProductSize Size { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; }
}
