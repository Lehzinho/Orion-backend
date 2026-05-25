using OrionPOS.Application.Auth;

namespace OrionPOS.Infra.Security;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public bool Verify(string plainTextPassword, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(plainTextPassword, passwordHash);
    }
}
