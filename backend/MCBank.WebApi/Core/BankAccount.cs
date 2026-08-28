using System.Text.Json.Serialization;

namespace MCBank.WebApi.Core;

public class BankAccount
{
    public Guid Guid { get; init; } = Guid.NewGuid();
    [JsonInclude] public decimal Balance { get; internal set; }
    public List<Transaction> Transactions { get; set; } = [];
}