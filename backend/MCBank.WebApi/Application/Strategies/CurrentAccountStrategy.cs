using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Entities;

namespace MCBank.WebApi.Application.Strategies;

public class CurrentAccountStrategy : IAccountStrategy
{
    public Result CanDeposit(decimal amount, Account account) =>
        amount <= 0
            ? Result.Failure("Нельзя пополнить текущий счет суммой меньше или равной нулю")
            : Result.Success();

    public Result CanWithdraw(decimal amount, Account account) =>
        amount > account.Balance ? Result.Failure("Недостаточно средств на текущем счете") : Result.Success();

    public Result CanTransfer(decimal amount, Account account) => CanWithdraw(amount, account);
}