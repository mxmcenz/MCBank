using MCBank.Cli;

Console.WriteLine("--- MCBank: Система управления счетом ---");

var bankAccount = new BankAccount();
var isWorking = true;

while (isWorking)
{
    CliUserInterface.PrintMenu();

    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine("Вы ничего не ввели! Попробуйте еще раз.");
        continue;
    }

    switch (input)
    {
        case "q":
            Console.WriteLine("Выход из приложения...");
            isWorking = false;
            break;
        case "1":
            bankAccount.ShowBalance();
            break;
        case "2":
            Console.Write("Введите сумму для пополнения счета: ");

            if (CliUserInterface.TryParseAmount(Console.ReadLine()!, out var amountToDeposit))
            {
                bankAccount.Deposit(amountToDeposit);
            }
            else
            {
                Console.WriteLine("Ошибка: Некорректный ввод суммы!");
            }

            break;
        case "3":
            Console.Write("Введите сумму для снятия со счета: ");
            if (CliUserInterface.TryParseAmount(Console.ReadLine()!, out var amountToWithdraw))
            {
                bankAccount.Withdraw(amountToWithdraw);
            }
            else
            {
                Console.WriteLine("Ошибка: Некорректный ввод суммы!");
            }

            break;
        case "4":
            CliUserInterface.PrintTransactions(bankAccount.GetTransactions());
            break;
        default:
            Console.WriteLine("Некорректный ввод! Попробуйте еще раз.");
            break;
    }
}