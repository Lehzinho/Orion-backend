namespace OrionPOS.Application.Auth;

public sealed class LoginUseCase(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService)
{
    public async Task<LoginResponseDto?> ExecuteAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = await userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null)
        {
            return null;
        }

        var isValid = passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isValid)
        {
            return null;
        }

        var token = jwtTokenService.GenerateAccessToken(user);
        return new LoginResponseDto(token);
    }
}
