using Ecommerce.Api.DTOs.Auth;
using Ecommerce.Api.Models;

namespace Ecommerce.Api.Mappings;

public static class AuthMappingExtensions
{
    public static UserResponseDto ToResponseDto(this User user)
    {
        return new UserResponseDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName);
    }

    public static LoginUserResponseDto ToLoginResponseDto(this User user, string token)
    {
        return new LoginUserResponseDto(token, user.ToResponseDto());
    }
}
