namespace MCBank.Cli;

public class BankAccount
{
    private decimal _balance;
    private readonly List<Transaction> _transactions;
    private readonly FileManager _fileManager = new();

    public BankAccount()
    {
        _transactions = _fileManager.Load();
        _balance = _transactions.Sum(t => t.Type == TransactionType.Deposit ? t.Amount : -t.Amount);
    }

    public List<Transaction> GetTransactions() => _transactions;

    public void ShowBalance()
    {
        Console.WriteLine(new string('-', 15));
        Console.WriteLine($"Ваш баланс: {_balance}");
        Console.WriteLine(new string('-', 15));
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            Console.WriteLine(new string('-', 15));
            Console.WriteLine("Ошибка: Количество денег для пополнения счета не может меньше или равно нулю!");
            Console.WriteLine(new string('-', 15));
            return;
        }

        _balance += amount;

        var transaction = new Transaction()
        {
            Amount = amount,
            Type = TransactionType.Deposit,
            CreatedAt = DateTime.UtcNow
        };

        _transactions.Add(transaction);
        _fileManager.Save(_transactions);

        Console.WriteLine(new string('-', 15));
        Console.WriteLine($"Баланс пополнен успешно! Новый баланс: {_balance}");
        Console.WriteLine(new string('-', 15));
    }

    public void Withdraw(decimal amount)
    {
        if (amount > _balance)
        {
            Console.WriteLine(new string('-', 15));
            Console.WriteLine("Ошибка: Недостаточно средств на счете для снятия денег!");
            Console.WriteLine(new string('-', 15));
            return;
        }

        _balance -= amount;

        var transaction = new Transaction()
        {
            Amount = amount,
            Type = TransactionType.Withdraw,
            CreatedAt = DateTime.UtcNow
        };

        _transactions.Add(transaction);
        _fileManager.Save(_transactions);

        Console.WriteLine(new string('-', 15));
        Console.WriteLine($"Снятие с баланса прошло успешно! Новый баланс: {_balance}");
        Console.WriteLine(new string('-', 15));
    }
}