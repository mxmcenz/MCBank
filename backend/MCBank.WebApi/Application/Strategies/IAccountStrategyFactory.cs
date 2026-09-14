using MCBank.WebApi.Core.Enums;

namespace MCBank.WebApi.Application.Strategies;

public interface IAccountStrategyFactory
{
    IAccountStrategy GetStrategy(AccountType accountType);
}