using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Entities;

namespace MCBank.WebApi.Application.Strategies;

public interface IAccountStrategy
{
    Result CanDeposit(decimal amount);
    Result CanWithdraw(decimal amount, Account account);
    Result CanTransfer(decimal amount, Account account);
}