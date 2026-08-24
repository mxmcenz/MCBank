namespace MCBank.Cli;

public sealed record Transaction
{
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public DateTime CreatedAt { get; init; }
}