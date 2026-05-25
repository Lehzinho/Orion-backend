namespace OrionPOS.Application.Auth;

public sealed record LoginRequestDto(string Email, string Password);

public sealed record LoginResponseDto(string AccessToken);

public sealed record MeResponseDto(Guid Id, string Email, DateTime CreatedAt);
