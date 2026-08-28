using MCBank.WebApi.Core;
using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Enums;
using MCBank.WebApi.Core.Interfaces;
using MCBank.WebApi.Infrastructure.Interfaces;

namespace MCBank.WebApi.Application;

public class BankService : IBankService
{
    private readonly IStorage _storage;
    private readonly List<Account> _bankAccounts;

    public BankService(IStorage storage)
    {
        _storage = storage;
        _bankAccounts = _storage.Load();
    }

    public Task<Result<Account>> GetAccountByIdAsync(int id)
    {
        var account = _bankAccounts.FirstOrDefault(acc => acc.Id == id);

        if (account == null)
        {
            return Task.FromResult(Result<Account>.Failure("Счет не найден"));
        }

        return Task.FromResult(Result<Account>.Success(account));
    }

    public Task<Result<List<Account>>> GetAllAccountsAsync() =>
        Task.FromResult(Result<List<Account>>.Success(_bankAccounts));

    public async Task<Result<Account>> CreateAccountAsync()
    {
        var nextId = _bankAccounts.Count > 0 ? _bankAccounts.Max(a => a.Id) + 1 : 1;
        var randomDigits = string.Concat(Enumerable.Range(0, 18).Select(_ => Random.Shared.Next(0, 10)));
        var iban = $"KZ{randomDigits}";

        var account = new Account
        {
            Id = nextId,
            Iban = iban
        };

        _bankAccounts.Add(account);
        await _storage.SaveAsync(_bankAccounts);

        return Result<Account>.Success(account);
    }

    public async Task<Result> DepositAsync(int accountId, decimal amount)
    {
        if (amount <= 0)
        {
            return Result.Failure("Сумма не может быть меньше или равна 0");
        }

        var account = _bankAccounts.FirstOrDefault(acc => acc.Id == accountId);

        if (account == null)
        {
            return Result.Failure("Счет не найден");
        }

        account.Balance += amount;
        account.Transactions.Add(new Transaction
        {
            Amount = amount,
            Type = TransactionType.Deposit,
            CreatedAt = DateTime.UtcNow
        });

        await _storage.SaveAsync(_bankAccounts);
        return Result.Success();
    }

    public async Task<Result> WithdrawAsync(int accountId, decimal amount)
    {
        if (amount <= 0)
        {
            return Result.Failure("Сумма не может быть меньше или равна 0");
        }

        var account = _bankAccounts.FirstOrDefault(acc => acc.Id == accountId);

        if (account == null)
        {
            return Result.Failure("Счет не найден");
        }

        if (amount > account.Balance)
        {
            return Result.Failure("Недостаточно средств");
        }

        account.Balance -= amount;
        account.Transactions.Add(new Transaction
        {
            Amount = amount,
            Type = TransactionType.Withdraw,
            CreatedAt = DateTime.UtcNow
        });

        await _storage.SaveAsync(_bankAccounts);
        return Result.Success();
    }

    public async Task<Result> TransferAsync(int fromAccountId, int toAccountId, decimal amount)
    {
        if (amount <= 0)
        {
            return Result.Failure("Сумма не может быть меньше или равна 0");
        }

        var fromAccount = _bankAccounts.FirstOrDefault(acc => acc.Id == fromAccountId);

        if (fromAccount == null)
        {
            return Result.Failure("Счет не найден");
        }

        if (amount > fromAccount.Balance)
        {
            return Result.Failure("Недостаточно средств");
        }

        var toAccount = _bankAccounts.FirstOrDefault(acc => acc.Id == toAccountId);

        if (toAccount == null)
        {
            return Result.Failure("Счет не найден");
        }

        fromAccount.Balance -= amount;
        toAccount.Balance += amount;

        fromAccount.Transactions.Add(new Transaction
        {
            Amount = amount,
            Type = TransactionType.Withdraw,
            CreatedAt = DateTime.UtcNow
        });

        toAccount.Transactions.Add(new Transaction
        {
            Amount = amount,
            Type = TransactionType.Deposit,
            CreatedAt = DateTime.UtcNow
        });

        await _storage.SaveAsync(_bankAccounts);

        return Result.Success();
    }
}