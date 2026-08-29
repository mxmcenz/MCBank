using MCBank.WebApi.Core.Enums;

namespace MCBank.WebApi.Core;

public sealed record Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public DateTime CreatedAt { get; init; }
}