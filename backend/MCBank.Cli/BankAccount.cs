namespace MCBank.Cli;

public class BankAccount
{
    public Guid Guid { get; init; } = Guid.NewGuid();
    public decimal Balance { get; internal set; }
    public List<Transaction> Transactions { get; set; } = [];
}