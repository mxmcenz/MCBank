namespace MCBank.WebApi.Core;

public sealed record Account
{
    public int Id { get; init; }
    public string Iban { get; init; } = string.Empty;
    public decimal Balance { get; set; }
    
    public int UserId { get; init; }
    public User User { get; init; } = null!;
    public bool IsDeleted { get; set; }
}