using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Entities;

namespace MCBank.WebApi.Application.Strategies;

public class SavingsFixedAccountStrategy : IAccountStrategy
{
    public Result CanDeposit(decimal amount, Account account) =>
        amount <= 0
            ? Result.Failure("Нельзя пополнить фиксированный сберегательный счет суммой меньше или равной нулю")
            : Result.Success();

    public Result CanWithdraw(decimal amount, Account account) =>
        Result.Failure("Нельзя снять с фиксированного сберегательного счета");

    public Result CanTransfer(decimal amount, Account account) =>
        Result.Failure("Нельзя сделать перевод с фиксированного сберегательного счета");

    public decimal CalculateInterest(decimal balance, decimal annualRate) => balance * (annualRate / 100) / 12;

    public bool IsInterestDue(DateTime? lastAppliedAt, DateTime now) =>
        lastAppliedAt == null || lastAppliedAt.Value.AddMonths(1) <= now;
}