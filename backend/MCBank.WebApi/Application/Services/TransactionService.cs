using MCBank.WebApi.Application.Interfaces;
using MCBank.WebApi.Application.Strategies;
using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Entities;
using MCBank.WebApi.Core.Enums;
using MCBank.WebApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MCBank.WebApi.Application.Services;

public class TransactionService(AppDbContext dbContext, IAccountStrategyFactory accountFactory) : ITransactionService
{
    public async Task<Result> DepositAsync(int accountId, int currentUserId, decimal amount)
    {
        var account = await dbContext.Accounts.FindAsync(accountId);

        if (account == null)
            return Result.Failure("Счет не найден", ErrorType.NotFound);

        if (account.UserId != currentUserId)
            return Result.Failure("Доступ запрещен", ErrorType.Forbidden);

        if (account.Type == AccountType.SavingsFixed)
        {
            var hasExistingDeposit = await dbContext.Transactions
                .AnyAsync(t => t.AccountId == accountId && t.Type == TransactionType.Deposit);
            if (hasExistingDeposit)
                return Result.Failure("Фиксированный сберегательный счет можно пополнить только один раз",
                    ErrorType.Conflict);
        }

        var strategy = accountFactory.GetStrategy(account.Type);
        var validationResult = strategy.CanDeposit(amount, account);

        if (validationResult.IsFailure)
            return validationResult;

        account.Balance += amount;

        var transaction = new Transaction
        {
            AccountId = accountId,
            Amount = amount,
            Type = TransactionType.Deposit,
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.Transactions.AddAsync(transaction);
        await dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> WithdrawAsync(int accountId, int currentUserId, decimal amount)
    {
        var account = await dbContext.Accounts.FindAsync(accountId);

        if (account == null)
            return Result.Failure("Счет не найден", ErrorType.NotFound);

        if (account.UserId != currentUserId)
            return Result.Failure("Доступ запрещен", ErrorType.Forbidden);

        var strategy = accountFactory.GetStrategy(account.Type);
        var validationResult = strategy.CanWithdraw(amount, account);

        if (validationResult.IsFailure)
            return validationResult;

        account.Balance -= amount;

        var transaction = new Transaction
        {
            AccountId = accountId,
            Amount = amount,
            Type = TransactionType.Withdraw,
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.Transactions.AddAsync(transaction);
        await dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> TransferAsync(int fromAccountId, int toAccountId, int currentUserId, decimal amount)
    {
        var fromAccount = await dbContext.Accounts.FindAsync(fromAccountId);

        if (fromAccount == null)
            return Result.Failure("Счет отправителя не найден", ErrorType.NotFound);

        if (fromAccount.UserId != currentUserId)
            return Result.Failure("Доступ запрещен", ErrorType.Forbidden);

        var toAccount = await dbContext.Accounts.FindAsync(toAccountId);

        if (toAccount == null)
            return Result.Failure("Счет получателя не найден", ErrorType.NotFound);

        var fromAccountStrategy = accountFactory.GetStrategy(fromAccount.Type);
        var fromAccountValidationResult = fromAccountStrategy.CanTransfer(amount, fromAccount);

        if (fromAccountValidationResult.IsFailure)
            return fromAccountValidationResult;

        var toAccountStrategy = accountFactory.GetStrategy(toAccount.Type);
        var toAccountValidationResult = toAccountStrategy.CanDeposit(amount, toAccount);

        if (toAccountValidationResult.IsFailure)
            return toAccountValidationResult;

        fromAccount.Balance -= amount;
        toAccount.Balance += amount;

        var fromTransaction = new Transaction
        {
            AccountId = fromAccountId,
            Amount = amount,
            Type = TransactionType.Withdraw,
            CreatedAt = DateTime.UtcNow
        };

        var toTransaction = new Transaction
        {
            AccountId = toAccountId,
            Amount = amount,
            Type = TransactionType.Deposit,
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.Transactions.AddAsync(fromTransaction);
        await dbContext.Transactions.AddAsync(toTransaction);
        await dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<Transaction>>> GetTransactionHistoryAsync(int accountId, int currentUserId)
    {
        var account = await dbContext.Accounts.FindAsync(accountId);

        if (account == null)
            return Result<List<Transaction>>.Failure("Счет не найден", ErrorType.NotFound);

        if (account.UserId != currentUserId)
            return Result<List<Transaction>>.Failure("Доступ запрещен", ErrorType.Forbidden);

        var transactions = await dbContext.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Result<List<Transaction>>.Success(transactions);
    }
}