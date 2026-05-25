namespace OrionPOS.Application.Auth;

public interface IPasswordHasher
{
    bool Verify(string plainTextPassword, string passwordHash);
}
