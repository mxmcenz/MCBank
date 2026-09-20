using MCBank.WebApi.Core.Enums;

namespace MCBank.WebApi.Infrastructure.Settings;

public sealed record SavingsSettings
{
    public List<SavingsPlan> Plans { get; init; } = [];
}

public sealed record SavingsPlan
{
    public string Name { get; init; } = string.Empty;
    public decimal InterestRate { get; init; }
    public int TermMonths { get; init; }
    public AccountType Type { get; init; }
}