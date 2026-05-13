namespace Ecommerce.Api.DTOs.Auth;

public sealed record UserResponseDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName);
