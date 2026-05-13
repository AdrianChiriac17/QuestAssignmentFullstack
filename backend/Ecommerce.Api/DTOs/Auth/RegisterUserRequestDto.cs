using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.DTOs.Auth;

public class RegisterUserRequestDto
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 8)]
    [RegularExpression(
        "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
}
