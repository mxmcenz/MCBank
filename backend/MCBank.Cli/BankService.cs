using MCBank.Cli.Enums;

namespace MCBank.Cli;

public class BankService
{
    private readonly List<BankAccount> _bankAccounts;
    private readonly FileManager _fileManager = new();

    public List<BankAccount> GetAccounts() => _bankAccounts;


    public BankService()
    {
        _bankAccounts = _fileManager.Load();
    }
    
    public bool Deposit(BankAccount account, decimal amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        account.Balance += amount;

        var transaction = new Transaction()
        {
            Amount = amount,
            Type = TransactionType.Deposit,
            CreatedAt = DateTime.UtcNow
        };

        account.Transactions.Add(transaction);
        _fileManager.Save(_bankAccounts);
        return true;
    }

    public bool Withdraw(BankAccount account, decimal amount)
    {
        if (amount > account.Balance)
        {
            return false;
        }

        account.Balance -= amount;

        var transaction = new Transaction()
        {
            Amount = amount,
            Type = TransactionType.Withdraw,
            CreatedAt = DateTime.UtcNow
        };

        account.Transactions.Add(transaction);
        _fileManager.Save(_bankAccounts);
        return true;
    }

    public void AddAccount(BankAccount account)
    {
        _bankAccounts.Add(account);
        _fileManager.Save(_bankAccounts);
    }

    public bool Transfer(BankAccount fromAccount, BankAccount toAccount, decimal amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (amount > fromAccount.Balance)
        {
            return false;
        }

        Withdraw(fromAccount, amount);
        Deposit(toAccount, amount);
        
        _fileManager.Save(_bankAccounts);
        return true;
    }
}