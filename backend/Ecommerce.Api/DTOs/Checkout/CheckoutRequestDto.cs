using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Checkout;

public class CheckoutRequestDto
{
    [Required]
    [MinLength(1)]
    public List<CheckoutItemRequestDto> Items { get; set; } = [];

    [Required]
    [StringLength(150)]
    public string RecipientName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string AddressLine { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;
}
