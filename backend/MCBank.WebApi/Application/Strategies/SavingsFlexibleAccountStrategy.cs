using MCBank.WebApi.Core.Common;

namespace MCBank.WebApi.Application.Strategies;

public class SavingsFlexibleAccountStrategy : IAccountStrategy
{
    public Result CanDeposit(decimal amount) =>
        amount <= 0
            ? Result.Failure("Нельзя пополнить сберегательный гибкий счет суммой меньше или равной нулю")
            : Result.Success();

    public Result CanWithdraw(decimal amount, decimal balance) =>
        amount > balance ? Result.Failure("Недостаточно средств на сберегательном гибком счете") : Result.Success();

    public Result CanTransfer(decimal amount, decimal balance) => CanWithdraw(amount, balance);
}