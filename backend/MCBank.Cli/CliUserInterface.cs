namespace MCBank.Cli;

public static class CliUserInterface
{
    public static void PrintMenu()
    {
        Console.WriteLine(new string('-', 15));
        Console.WriteLine("Главное меню:");
        Console.WriteLine("1. Показать баланс");
        Console.WriteLine("2. Пополнить счет");
        Console.WriteLine("3. Снять деньги");
        Console.WriteLine("4. Выписка по счету");
        Console.WriteLine("(q Выйти)");
        Console.WriteLine(new string('-', 15));
    }

    public static void PrintTransactions(List<Transaction> transactions)
    {
        if (transactions.Count == 0)
        {
            Console.WriteLine(new string('-', 15));
            Console.WriteLine("Транзакций по данному счету отсутствуют");
            Console.WriteLine(new string('-', 15));
            return;
        }
        
        Console.WriteLine(new string('-', 15));
        Console.WriteLine("--- Выписка по счету ---");
        Console.WriteLine("#    Сумма    Тип   Дата   Время");
        for (var i = 0; i < transactions.Count; i++)
        {
            var transaction = transactions[i];
            Console.WriteLine(
                $"[{i + 1}] {transaction.Amount} " +
                $"{transaction.Type.ToString()} " +
                $"{transaction.CreatedAt.ToShortDateString()} " +
                $"{transaction.CreatedAt.ToShortTimeString()}");
        }
        Console.WriteLine(new string('-', 15));
    }

    public static bool TryParseAmount(string input, out decimal amount)
    {
        var isParsed = decimal.TryParse(input, out amount);

        if (isParsed && amount > 0)
        {
            return true;
        }

        amount = 0;
        return false;
    }
}