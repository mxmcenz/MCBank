using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Entities;

namespace MCBank.WebApi.Application.Strategies;

public class SavingsFlexibleAccountStrategy : IAccountStrategy
{
    public Result CanDeposit(decimal amount, Account account) =>
        amount <= 0
            ? Result.Failure("Нельзя пополнить сберегательный гибкий счет суммой меньше или равной нулю")
            : Result.Success();

    public Result CanWithdraw(decimal amount, Account account) => amount > account.Balance
        ? Result.Failure("Недостаточно средств на сберегательном гибком счете")
        : Result.Success();

    public Result CanTransfer(decimal amount, Account account) => CanWithdraw(amount, account);
    
    public decimal CalculateInterest(decimal balance, decimal annualRate) => balance * (annualRate / 100) / 365;

    public bool IsInterestDue(DateTime? lastAppliedAt, DateTime now) =>
        lastAppliedAt == null || lastAppliedAt.Value.Date < now.Date;
}