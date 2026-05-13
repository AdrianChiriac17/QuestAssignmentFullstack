using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Auth;

public class LoginUserRequestDto
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;
}
