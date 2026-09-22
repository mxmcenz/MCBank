using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Entities;

namespace MCBank.WebApi.Application.Strategies;

public interface IAccountStrategy
{
    Result CanDeposit(decimal amount, Account account);
    Result CanWithdraw(decimal amount, Account account);
    Result CanTransfer(decimal amount, Account account);
    decimal CalculateInterest(decimal balance, decimal annualRate);
    bool IsInterestDue(DateTime? lastAppliedAt, DateTime now);
}