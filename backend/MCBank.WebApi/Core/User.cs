namespace MCBank.WebApi.Core;

public sealed record User
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    
    public List<Account> Accounts { get; init; } = [];
    public bool IsDeleted { get; set; }
}