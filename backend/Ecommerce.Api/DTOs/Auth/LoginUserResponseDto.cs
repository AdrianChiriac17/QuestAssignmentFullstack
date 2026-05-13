namespace Ecommerce.Api.DTOs.Auth;

public sealed record LoginUserResponseDto(
    string Token,
    UserResponseDto User);
