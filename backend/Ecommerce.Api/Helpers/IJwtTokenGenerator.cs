using Ecommerce.Api.Models;

namespace Ecommerce.Api.Helpers;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
