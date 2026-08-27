using MCBank.Cli.Enums;
using Spectre.Console;

namespace MCBank.Cli;

public class CliUserInterface(BankService bankService)
{
    public BankAccount? RunMainLoop()
    {
        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<MainMenuOptions>()
                    .Title("[yellow]Главное меню[/]")
                    .AddChoices(MainMenuOptions.CreateAccount, MainMenuOptions.SelectAccount, MainMenuOptions.Exit)
                    .UseConverter(option => option switch
                    {
                        MainMenuOptions.CreateAccount => "Создать новый счет",
                        MainMenuOptions.SelectAccount => "Выбрать существующий счет",
                        MainMenuOptions.Exit => "Выход",
                        _ => option.ToString()
                    }));

            switch (choice)
            {
                case MainMenuOptions.CreateAccount:
                    var newAccount = new BankAccount();
                    bankService.AddAccount(newAccount);
                    AnsiConsole.MarkupLine($"[green]Счет успешно создан![/] ID: {newAccount.Guid}");
                    return newAccount;
                case MainMenuOptions.SelectAccount:
                    var accounts = bankService.GetAccounts();
                    if (accounts.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[red]У вас еще нет счетов![/]");
                        continue;
                    }

                    return AnsiConsole.Prompt(
                        new SelectionPrompt<BankAccount>()
                            .Title("Выберите счет:")
                            .PageSize(10)
                            .AddChoices(accounts)
                            .UseConverter(acc =>
                                $"Счет: [blue]{Markup.Escape(acc.Guid.ToString())}[/] | Баланс: [green]{acc.Balance}$[/]"));
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
                    bankService.Deposit(selectedAccount, AnsiConsole.Prompt(
                        new TextPrompt<decimal>("Введите сумму:")
                            .ValidationErrorMessage("[red]Ошибка:[/] Введите положительное число")
                            .Validate(value => value > 0)));
                    break;
                case AccountMenuOptions.Withdraw:
                    bankService.Withdraw(selectedAccount, AnsiConsole.Prompt(
                        new TextPrompt<decimal>("Введите сумму:")
                            .ValidationErrorMessage("[red]Ошибка:[/] Введите положительное число")
                            .Validate(value => value > 0)));
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
        }
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
}