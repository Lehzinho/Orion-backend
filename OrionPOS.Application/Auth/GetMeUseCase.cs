namespace OrionPOS.Application.Auth;

public sealed class GetMeUseCase(IUserRepository userRepository)
{
    public async Task<MeResponseDto?> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return new MeResponseDto(
            Id: user.Id,
            Email: user.Email,
            CreatedAt: user.CreatedAt
        );
    }
}
