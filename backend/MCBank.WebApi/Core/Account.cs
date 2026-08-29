using System.Text.Json.Serialization;

namespace MCBank.WebApi.Core;

public class Account
{
    public int Id { get; set; }
    public string Iban { get; set; } = string.Empty;
    public decimal Balance { get; internal set; }
}