using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Application.Interfaces;
using MCBank.WebApi.Application.Strategies;
using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Entities;
using MCBank.WebApi.Core.Enums;
using MCBank.WebApi.Infrastructure;
using MCBank.WebApi.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MCBank.WebApi.Application;

public class BankService(
    AppDbContext dbContext,
    IAccountStrategyFactory accountFactory,
    IOptions<SavingsSettings> savingsOptions) : IBankService
{
    private readonly SavingsSettings _savingsSettings = savingsOptions.Value;

    public async Task<Result<AccountResponse>> GetAccountByIdAsync(int accountId, int currentUserId)
    {
        var account = await dbContext.Accounts.FindAsync(accountId);

        if (account == null)
            return Result<AccountResponse>.Failure("Счет не найден", ErrorType.NotFound);

        if (account.UserId != currentUserId)
            return Result<AccountResponse>.Failure("Доступ запрещен", ErrorType.Forbidden);

        var dto = new AccountResponse(
            account.Id,
            account.Iban,
            account.Balance,
            account.Type,
            account.ExpirationDate,
            account.InterestRate,
            account.CreatedAt);

        return Result<AccountResponse>.Success(dto);
    }

    public async Task<Result<List<AccountResponse>>> GetAllAccountsAsync(int userId)
    {
        var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .ToListAsync();

        var dto = accounts.Select(a => new AccountResponse(
            a.Id, a.Iban, a.Balance, a.Type, a.ExpirationDate, a.InterestRate, a.CreatedAt)).ToList();

        return Result<List<AccountResponse>>.Success(dto);
    }

    public Task<Result<List<SavingsPlan>>> GetSavingsPlansAsync() => 
        Task.FromResult(Result<List<SavingsPlan>>.Success(_savingsSettings.Plans));

    public async Task<Result<AccountResponse>> CreateAccountAsync(int userId, AccountType type, int? termMonths)
    {
        var userExists = await dbContext.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            return Result<AccountResponse>.Failure("Пользователь не найден", ErrorType.NotFound);

        var iban = $"KZ{string.Concat(Enumerable.Range(0, 18).Select(_ => Random.Shared.Next(0, 10)))}";
        decimal? interestRate = null;
        DateTime? expirationDate = null;

        if (type != AccountType.Current)
        {
            var plan = _savingsSettings.Plans.FirstOrDefault(p => p.Type == type && p.TermMonths == termMonths);
            if (plan == null)
                return Result<AccountResponse>.Failure("План не найден", ErrorType.NotFound);

            interestRate = plan.InterestRate;
            expirationDate = DateTime.UtcNow.AddMonths(plan.TermMonths);
        }

        var account = new Account
        {
            Iban = iban,
            UserId = userId,
            Balance = 0,
            Type = type,
            ExpirationDate = expirationDate,
            InterestRate = interestRate,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        var dto = new AccountResponse(
            account.Id,
            account.Iban,
            account.Balance,
            account.Type,
            account.ExpirationDate,
            account.InterestRate,
            account.CreatedAt);

        return Result<AccountResponse>.Success(dto);
    }

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

    public async Task<Result<int>> GetAccountIdByIbanAsync(string iban)
    {
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Iban == iban);
        return account == null
            ? Result<int>.Failure("Счет не найден", ErrorType.NotFound)
            : Result<int>.Success(account.Id);
    }

    public async Task<Result> DeleteAccount(int accountId, int currentUserId)
    {
        var account = await dbContext.Accounts.FindAsync(accountId);

        if (account == null)
            return Result.Failure("Счет не найден", ErrorType.NotFound);

        if (account.UserId != currentUserId)
            return Result.Failure("Доступ запрещен", ErrorType.Forbidden);

        account.IsDeleted = true;
        await dbContext.SaveChangesAsync();

        return Result.Success();
    }
}