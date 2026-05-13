using Ecommerce.Api.DTOs.Auth;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Mappings;
using Ecommerce.Api.Models;
using Ecommerce.Api.Repositories.Interfaces;
using Ecommerce.Api.Services.Interfaces;

namespace Ecommerce.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository userRepository;
    private readonly IPasswordHasher passwordHasher;
    private readonly IJwtTokenGenerator jwtTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        this.userRepository = userRepository;
        this.passwordHasher = passwordHasher;
        this.jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<UserResponseDto> RegisterAsync(
        RegisterUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (await userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("An account with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim(),
            PasswordHash = passwordHasher.HashPassword(request.Password),
            FirstName = CapitalizeFirstLetter(request.FirstName),
            LastName = CapitalizeFirstLetter(request.LastName),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await userRepository.CreateAsync(user, cancellationToken);

        return user.ToResponseDto();
    }

    public async Task<LoginUserResponseDto?> LoginAsync(
        LoginUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is null || 
            passwordHasher.VerifyPassword(request.Password, user.PasswordHash) == false)
        {
            return null;
        }

        var token = jwtTokenGenerator.GenerateToken(user);

        return user.ToLoginResponseDto(token);
    }

    private static string CapitalizeFirstLetter(string value)
    {
        var trimmedValue = value.Trim();

        return trimmedValue.Length switch
        {
            0 => trimmedValue,
            1 => trimmedValue.ToUpperInvariant(),
            _ => char.ToUpperInvariant(trimmedValue[0]) + trimmedValue[1..]
        };
    }
}
