using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Entities;

namespace MCBank.WebApi.Application.Strategies;

public class SavingsFixedAccountStrategy : IAccountStrategy
{
    public Result CanDeposit(decimal amount) =>
        Result.Failure("Нельзя пополнить фиксированный сберегательный счет");

    public Result CanWithdraw(decimal amount, Account account) =>
        Result.Failure("Нельзя снять с фиксированного сберегательного счета");

    public Result CanTransfer(decimal amount, Account account) =>
        Result.Failure("Нельзя сделать перевод с фиксированного сберегательного счета");
}