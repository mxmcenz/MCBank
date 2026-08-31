using MCBank.WebApi.Core.Enums;

namespace MCBank.WebApi.Core;

public sealed record Transaction
{
    public int Id { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public DateTime CreatedAt { get; init; }
    
    public int AccountId { get; init; }
    public Account Account { get; init; } = null!;
    public bool IsDeleted { get; set; }
}