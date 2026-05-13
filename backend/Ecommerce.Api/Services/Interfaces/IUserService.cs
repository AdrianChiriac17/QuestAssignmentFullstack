using Ecommerce.Api.DTOs.Auth;

namespace Ecommerce.Api.Services.Interfaces;

public interface IUserService
{
    Task<UserResponseDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
