using OrionPOS.Domain.Auth;

namespace OrionPOS.Application.Auth;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
}
