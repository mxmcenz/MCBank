using MCBank.WebApi.Core;
using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Enums;
using MCBank.WebApi.Core.Interfaces;
using MCBank.WebApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MCBank.WebApi.Application;

public class BankService(AppDbContext dbContext) : IBankService
{
    public async Task<Result<Account>> GetAccountByIdAsync(int id)
    {
        var account = await dbContext.Accounts.FindAsync(id);

        return account == null ? Result<Account>.Failure("Счет не найден") : Result<Account>.Success(account);
    }

    public async Task<Result<List<Account>>> GetAllAccountsAsync()
    {
        var accounts = await dbContext.Accounts
            .AsNoTracking()
            .ToListAsync();

        return Result<List<Account>>.Success(accounts);
    }

    public async Task<Result<Account>> CreateAccountAsync()
    {
        var randomDigits = string.Concat(Enumerable.Range(0, 18).Select(_ => Random.Shared.Next(0, 10)));
        var iban = $"KZ{randomDigits}";

        var account = new Account
        {
            Iban = iban
        };

        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        return Result<Account>.Success(account);
    }

    public async Task<Result> DepositAsync(int accountId, decimal amount)
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

    public async Task<Result> WithdrawAsync(int accountId, decimal amount)
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

    public async Task<Result> TransferAsync(int fromAccountId, int toAccountId, decimal amount)
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

    public async Task<Result<List<Transaction>>> GetTransactionHistoryAsync(int accountId)
    {
        var exists = await dbContext.Accounts
            .AsNoTracking()
            .AnyAsync(a => a.Id == accountId);

        if (!exists)
        {
            return Result<List<Transaction>>.Failure("Счет не найден");
        }

        var transactions = await dbContext.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Result<List<Transaction>>.Success(transactions);
    }

    public async Task<Result> DeleteAccount(int accountId)
    {
        var account = await dbContext.Accounts.FindAsync(accountId);

        if (account == null)
        {
            return Result.Failure("Счет не найден");
        }

        account.IsDeleted = true;
        await dbContext.SaveChangesAsync();
        
        return Result.Success();
    }
}