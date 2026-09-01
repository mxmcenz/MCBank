namespace MCBank.WebApi.Application.DTOs;

public sealed record TokenPairDto
{
    public string AccessToken { get; init; } = string.Empty;
    public int AccessTokenExpiresIn { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
    public int RefreshTokenExpiresIn { get; init; }
}