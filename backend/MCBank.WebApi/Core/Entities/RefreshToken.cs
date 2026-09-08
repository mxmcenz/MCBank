namespace MCBank.WebApi.Core.Entities;

public sealed record RefreshToken
{
    public int Id { get; init; }
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public bool IsRevoked { get; init; }
    
    public int UserId { get; init; }
    public User User { get; init; }
}