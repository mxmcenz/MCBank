using MCBank.WebApi.Core.Enums;

namespace MCBank.WebApi.Application.Strategies;

public class AccountStrategyFactory : IAccountStrategyFactory
{
    public IAccountStrategy GetStrategy(AccountType accountType)
    {
        return accountType switch
        {
            AccountType.Current => new CurrentAccountStrategy(),
            AccountType.SavingsFixed => new SavingsFixedAccountStrategy(),
            AccountType.SavingsFlexible => new SavingsFlexibleAccountStrategy(),
            AccountType.SavingsReplenishable => new SavingsReplenishableAccountStrategy(),
            _ => throw new ArgumentException($"Invalid account type: {accountType}")
        };
    }
}