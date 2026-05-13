using Ecommerce.Api.DTOs.Auth;

namespace Ecommerce.Api.Services.Interfaces;

public interface IAuthService
{
    Task<UserResponseDto> RegisterAsync(
        RegisterUserRequestDto request,
        CancellationToken cancellationToken = default);

    Task<LoginUserResponseDto?> LoginAsync(
        LoginUserRequestDto request,
        CancellationToken cancellationToken = default);
}
