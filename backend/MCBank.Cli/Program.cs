Console.WriteLine("--- MCBank: Система управления счетом ---");

decimal balance = 0;

var isWorking = true;

while (isWorking)
{
    PrintMenu();

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
            ShowBalance();
            break;
        case "2":
            Console.Write("Введите сумму для пополнения счета: ");
            var amountToDepositInput = Console.ReadLine();

            if (TryParseAmount(amountToDepositInput!, out decimal amountToDeposit))
            {
                Deposit(amountToDeposit);
            }
            else
            {
                Console.WriteLine("Ошибка: Некорректный ввод суммы!");
            }
            break;
        case "3":
            Console.Write("Введите сумму для снятия со счета: ");
            var amountToWithdrawInput = Console.ReadLine();
            if (TryParseAmount(amountToWithdrawInput!, out decimal amountToWithdraw))
            {
                Withdraw(amountToWithdraw);
            }
            else
            {
                Console.WriteLine("Ошибка: Некорректный ввод суммы!");
            }
            break;
        default:
            Console.WriteLine("Некорректный ввод! Попробуйте еще раз.");
            break;
    }

}

return;

void PrintMenu()
{
    Console.WriteLine(new string('-', 15));
    Console.WriteLine("Главное меню:");
    Console.WriteLine("1. Показать баланс");
    Console.WriteLine("2. Пополнить счет");
    Console.WriteLine("3. Снять деньги");
    Console.WriteLine("(q Выйти)");
    Console.WriteLine(new string('-', 15));
}

void ShowBalance()
{
    Console.WriteLine(new string('-', 15));
    Console.WriteLine($"Ваш баланс: {balance}");
    Console.WriteLine(new string('-', 15));
}

bool TryParseAmount(string input, out decimal amount)
{
    var isParsed = decimal.TryParse(input, out amount);

    if (isParsed && amount > 0)
    {
        return true;
    }

    amount = 0;
    return false;
}

void Deposit(decimal amount)
{
    if (amount <= 0)
    {
        Console.WriteLine(new string('-', 15));
        Console.WriteLine("Ошибка: Количество денег для пополнения счета не может меньше или равно нулю!");
        Console.WriteLine(new string('-', 15));
        return;
    }

    balance += amount;
    Console.WriteLine(new string('-', 15));
    Console.WriteLine($"Баланс пополнен успешно! Новый баланс: {balance}");
    Console.WriteLine(new string('-', 15));
}

void Withdraw(decimal amount)
{
    if (amount > balance)
    {
        Console.WriteLine(new string('-', 15));
        Console.WriteLine("Ошибка: Недостаточно средств на счете для снятия денег!");
        Console.WriteLine(new string('-', 15));
        return;
    }

    balance -= amount;
    Console.WriteLine(new string('-', 15));
    Console.WriteLine($"Снятие с баланса прошло успешно! Новый баланс: {balance}");
    Console.WriteLine(new string('-', 15));
}

