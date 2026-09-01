using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Application.Interfaces;
using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Entities;
using MCBank.WebApi.Core.Enums;
using MCBank.WebApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MCBank.WebApi.Application;

public class BankService(AppDbContext dbContext) : IBankService
{
    public async Task<Result<AccountResponse>> GetAccountByIdAsync(int accountId, int currentUserId)
    {
        var account = await dbContext.Accounts.FindAsync(accountId);

        if (account == null)
        {
            return Result<AccountResponse>.Failure("Счет не найден");
        }

        if (account.UserId != currentUserId)
        {
            return Result<AccountResponse>.Failure("Доступ запрещен");
        }

        var dto = new AccountResponse(account.Id, account.Iban, account.Balance);

        return Result<AccountResponse>.Success(dto);
    }

    public async Task<Result<List<AccountResponse>>> GetAllAccountsAsync(int userId)
    {
        var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .ToListAsync();

        var dto = accounts.Select(a => new AccountResponse(a.Id, a.Iban, a.Balance)).ToList();

        return Result<List<AccountResponse>>.Success(dto);
    }

    public async Task<Result<AccountResponse>> CreateAccountAsync(int userId)
    {
        var userExists = await dbContext.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            return Result<AccountResponse>.Failure("Пользователь не найден");

        var randomDigits = string.Concat(Enumerable.Range(0, 18).Select(_ => Random.Shared.Next(0, 10)));
        var iban = $"KZ{randomDigits}";

        var account = new Account
        {
            Iban = iban,
            UserId = userId,
            Balance = 0,
            IsDeleted = false
        };

        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        var dto = new AccountResponse(account.Id, account.Iban, account.Balance);

        return Result<AccountResponse>.Success(dto);
    }

    public async Task<Result> DepositAsync(int accountId, int currentUserId, decimal amount)
    {
        if (amount <= 0)
        {
            return Result.Failure("Сумма не может быть меньше или равна 0");
        }

        var account = await dbContext.Accounts.FindAsync(accountId);

        if (account == null)
        {
            return Result.Failure("Счет не найден");
        }

        if (account.UserId != currentUserId)
        {
            return Result.Failure("Доступ запрещен");
        }

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
        if (amount <= 0)
        {
            return Result.Failure("Сумма не может быть меньше или равна 0");
        }

        var account = await dbContext.Accounts.FindAsync(accountId);

        if (account == null)
        {
            return Result.Failure("Счет не найден");
        }

        if (account.UserId != currentUserId)
        {
            return Result.Failure("Доступ запрещен");
        }

        if (amount > account.Balance)
        {
            return Result.Failure("Недостаточно средств");
        }

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
        if (amount <= 0)
        {
            return Result.Failure("Сумма не может быть меньше или равна 0");
        }

        var fromAccount = await dbContext.Accounts.FindAsync(fromAccountId);

        if (fromAccount == null)
        {
            return Result.Failure("Счет отправителя не найден");
        }

        if (fromAccount.UserId != currentUserId)
        {
            return Result.Failure("Доступ запрещен");
        }

        var toAccount = await dbContext.Accounts.FindAsync(toAccountId);

        if (toAccount == null)
        {
            return Result.Failure("Счет получателя не найден");
        }

        if (amount > fromAccount.Balance)
        {
            return Result.Failure("Недостаточно средств");
        }

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
        {
            return Result<List<Transaction>>.Failure("Счет не найден");
        }

        if (account.UserId != currentUserId)
        {
            return Result<List<Transaction>>.Failure("Доступ запрещен");
        }

        var transactions = await dbContext.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Result<List<Transaction>>.Success(transactions);
    }

    public async Task<Result> DeleteAccount(int accountId, int currentUserId)
    {
        var account = await dbContext.Accounts.FindAsync(accountId);

        if (account == null)
        {
            return Result.Failure("Счет не найден");
        }

        if (account.UserId != currentUserId)
        {
            return Result.Failure("Доступ запрещен");
        }

        account.IsDeleted = true;
        await dbContext.SaveChangesAsync();

        return Result.Success();
    }
}