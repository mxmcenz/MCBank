using MCBank.Cli.Enums;
using Spectre.Console;

namespace MCBank.Cli;

public class CliUserInterface(BankService bankService)
{
    public BankAccount? RunMainLoop()
    {
        while (true)
        {
            AnsiConsole.Clear();
            DrawHeader();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<MainMenuOptions>()
                    .Title("[yellow]Главное меню[/]")
                    .AddChoices(MainMenuOptions.SelectAccount, MainMenuOptions.CreateAccount, MainMenuOptions.Exit)
                    .UseConverter(option => option switch
                    {
                        MainMenuOptions.SelectAccount => "Выбрать существующий счет",
                        MainMenuOptions.CreateAccount => "Создать новый счет",
                        MainMenuOptions.Exit => "Выход",
                        _ => option.ToString()
                    }));

            switch (choice)
            {
                case MainMenuOptions.SelectAccount:
                    var accounts = bankService.GetAccounts();
                    if (accounts.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[red]У вас еще нет счетов![/]");
                        WaitForEnter();
                        continue;
                    }

                    return AnsiConsole.Prompt(
                        new SelectionPrompt<BankAccount>()
                            .Title("Выберите счет:")
                            .PageSize(10)
                            .AddChoices(accounts)
                            .UseConverter(acc =>
                                $"Счет: [blue]{Markup.Escape(acc.Guid.ToString())}[/] | Баланс: [green]{acc.Balance}$[/]"));
                case MainMenuOptions.CreateAccount:
                    var newAccount = new BankAccount();
                    bankService.AddAccount(newAccount);
                    AnsiConsole.MarkupLine($"[green]Счет успешно создан![/] ID: {newAccount.Guid}");
                    WaitForEnter();
                    return newAccount;
                case MainMenuOptions.Exit:
                default:
                    return null;
            }
        }
    }

    public void RunAccountMenu(BankAccount selectedAccount)
    {
        while (true)
        {
            AnsiConsole.Clear();
            DrawHeader();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<AccountMenuOptions>()
                    .Title("[yellow]Выберите действие со счетом:[/]")
                    .AddChoices(AccountMenuOptions.ShowBalance, AccountMenuOptions.Deposit, AccountMenuOptions.Withdraw,
                        AccountMenuOptions.Transactions, AccountMenuOptions.Transfer, AccountMenuOptions.Back)
                    .UseConverter(option => option switch
                    {
                        AccountMenuOptions.ShowBalance => "Показать баланс",
                        AccountMenuOptions.Deposit => "Пополнить счет",
                        AccountMenuOptions.Withdraw => "Снять деньги",
                        AccountMenuOptions.Transactions => "Выписка по счету",
                        AccountMenuOptions.Transfer => "Перевод между счетами",
                        AccountMenuOptions.Back => "Вернуться в главное меню",
                        _ => option.ToString()
                    }));

            switch (choice)
            {
                case AccountMenuOptions.ShowBalance:
                    AnsiConsole.MarkupLine($"[bold]Баланс:[/] [green]{selectedAccount.Balance}$[/]");
                    break;
                case AccountMenuOptions.Deposit:
                    var depositPrompt = AnsiConsole.Prompt(
                        new TextPrompt<decimal>("Введите сумму:")
                            .ValidationErrorMessage("[red]Ошибка:[/] Введите положительное число")
                            .Validate(value => value > 0));


                    var deposited = bankService.Deposit(selectedAccount, depositPrompt);

                    AnsiConsole.MarkupLine(deposited
                        ? "[green]Счет успешно пополнен[/]"
                        : "[red]Ошибка:[/] Не удалось пополнить счет");

                    break;
                case AccountMenuOptions.Withdraw:
                    var withdrawPrompt = AnsiConsole.Prompt(
                        new TextPrompt<decimal>("Введите сумму:")
                            .ValidationErrorMessage("[red]Ошибка:[/] Введите положительное число")
                            .Validate(value => value > 0));

                    var withdrawed = bankService.Withdraw(selectedAccount, withdrawPrompt);

                    AnsiConsole.MarkupLine(withdrawed
                        ? "[green]Деньги успешно сняты со счета[/]"
                        : "[red]Ошибка:[/] Не удалось снять денег со счета");
                    break;
                case AccountMenuOptions.Transactions:
                    DisplayTransactions(selectedAccount);
                    break;
                case AccountMenuOptions.Transfer:
                    var otherAccounts = bankService.GetAccounts()
                        .Where(acc => acc.Guid != selectedAccount.Guid)
                        .ToList();

                    if (otherAccounts.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[red]Ошибка:[/] У вас нет других счетов для перевода");
                        break;
                    }

                    var targetAccount = AnsiConsole.Prompt(new SelectionPrompt<BankAccount>()
                        .Title("[yellow]Выберите счет для перевода:[/]")
                        .PageSize(10)
                        .AddChoices(otherAccounts)
                        .UseConverter(acc =>
                            $"Счет: [blue]{Markup.Escape(acc.Guid.ToString())}[/] | Баланс: [green]{acc.Balance}$[/]"));

                    var amount = AnsiConsole.Prompt(
                        new TextPrompt<decimal>("Введите сумму для перевода:")
                            .ValidationErrorMessage("[red]Ошибка:[/] Введите положительное число")
                            .Validate(value => value > 0));

                    var isSuccess = bankService.Transfer(selectedAccount, targetAccount, amount);

                    AnsiConsole.MarkupLine(isSuccess
                        ? "Деньги успешно переведены"
                        : "[red]Ошибка:[/] Не удалось перевести деньги");

                    break;
                case AccountMenuOptions.Back:
                default:
                    return;
            }

            WaitForEnter();
        }
    }

    private static void DrawHeader()
    {
        var header = new Rule("MCBank");
        AnsiConsole.Write(header);
    }

    private static void DisplayTransactions(BankAccount account)
    {
        var transactions = account.Transactions;

        if (transactions.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]История транзакций пуста.[/]");
            return;
        }

        var table = new Table()
            .Title($"Выписка по счету [blue]{account.Guid}[/]")
            .Border(TableBorder.Rounded);

        table.AddColumn("[grey]#[/]");
        table.AddColumn("Дата");
        table.AddColumn("Тип");
        table.AddColumn("Сумма");

        for (var i = 0; i < transactions.Count; i++)
        {
            var t = transactions[i];

            var typeText = t.Type == TransactionType.Deposit ? "[green]Пополнение[/]" : "[red]Снятие[/]";
            var amountText = t.Type == TransactionType.Deposit ? $"[green]+{t.Amount}[/]" : $"[red]-{t.Amount}[/]";

            table.AddRow(
                (i + 1).ToString(),
                t.CreatedAt.ToLocalTime().ToString("g"),
                typeText,
                amountText);
        }

        AnsiConsole.Write(table);
    }

    private static void WaitForEnter()
    {
        AnsiConsole.MarkupLine("\n[grey]Нажмите Enter, чтобы продолжить...[/]");
        Console.ReadLine();
    }
}