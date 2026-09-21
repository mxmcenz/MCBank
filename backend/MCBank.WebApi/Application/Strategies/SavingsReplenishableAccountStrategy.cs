using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Entities;

namespace MCBank.WebApi.Application.Strategies;

public class SavingsReplenishableAccountStrategy : IAccountStrategy
{
    public Result CanDeposit(decimal amount, Account account) =>
        amount <= 0
            ? Result.Failure("Нельзя пополнить сберегательный счет с пополнением суммой меньше или равной нулю")
            : Result.Success();

    public Result CanWithdraw(decimal amount, Account account) =>
        Result.Failure("Нельзя снять деньги с сберегательного счета с пополнением");

    public Result CanTransfer(decimal amount, Account account) =>
        Result.Failure("Нельзя сделать перевод денег с сберегательного счета с пополнением");
}