using MCBank.Cli;

var bankService = new BankService();
var ui = new CliUserInterface(bankService);

while (true)
{
    var selectedAccount = ui.RunMainLoop();

    if (selectedAccount == null)
        break;

    ui.RunAccountMenu(selectedAccount);
}