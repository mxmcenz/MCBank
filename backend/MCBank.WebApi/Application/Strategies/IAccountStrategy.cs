using MCBank.WebApi.Core.Common;

namespace MCBank.WebApi.Application.Strategies;

public interface IAccountStrategy
{
    Result CanDeposit(decimal amount);
    Result CanWithdraw(decimal amount, decimal balance);
    Result CanTransfer(decimal amount, decimal balance);
}