using MCBank.WebApi.Core.Enums;

namespace MCBank.WebApi.Core.Entities;

public sealed record Account
{
    public int Id { get; init; }
    public string Iban { get; init; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime? ExpirationDate { get; init; }
    public decimal? InterestRate { get; init; }
    public AccountType Type { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    public int UserId { get; init; }
    public User User { get; init; } = null!;
    public bool IsDeleted { get; set; }
}