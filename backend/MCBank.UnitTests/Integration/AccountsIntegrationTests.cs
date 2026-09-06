using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Core.Entities;
using MCBank.WebApi.Core.Enums;
using MCBank.WebApi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MCBank.UnitTests.Integration;

public class AccountsIntegrationTests(MCBankApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAccountById_WhenAuthenticated_ReturnsSuccess()
    {
        //Arrange
        await AuthenticateAsync();
        var accountResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        accountResponse.EnsureSuccessStatusCode();
        var createdAccount = await accountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        createdAccount.Should().NotBeNull();

        //Act
        var getAccountResponse = await Client.GetAsync($"/api/accounts/{createdAccount.Id}");

        //Assert
        getAccountResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var account = await getAccountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        account.Should().NotBeNull();
        account.Id.Should().Be(createdAccount.Id);
        account.Iban.Should().Be(createdAccount.Iban);
        account.Type.Should().Be(AccountType.Current);
    }

    [Fact]
    public async Task GetAccountById_OtherUserAccount_ReturnsForbidden()
    {
        //Arrange
        await AuthenticateAsync();
        var ownerAccountResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        ownerAccountResponse.EnsureSuccessStatusCode();
        var ownerAccount = await ownerAccountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        ownerAccount.Should().NotBeNull();
        var ownerAccountId = ownerAccount.Id;

        await AuthenticateAsync();

        //Act
        var response = await Client.GetAsync($"/api/accounts/{ownerAccountId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAccounts_WhenAuthenticated_ReturnsSuccessAndAllUserAccounts()
    {
        //Arrange
        var uniqueName = await AuthenticateAsync();
        await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));

        //Act
        var response = await Client.GetAsync("/api/accounts");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var accountsCount = await dbContext.Accounts.CountAsync(u => u.User.Username == uniqueName);
        accountsCount.Should().Be(3);
        var accounts = await response.Content.ReadFromJsonAsync<List<AccountResponse>>(JsonOptions);
        accounts.Should().NotBeNull();
        accounts.Count.Should().Be(accountsCount);
    }

    [Fact]
    public async Task GetAccounts_ShouldOnlyReturnOwnAccounts()
    {
        //Arrange
        await AuthenticateAsync();
        await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        await AuthenticateAsync();

        //Act
        var response = await Client.GetAsync("/api/accounts");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var accounts = await response.Content.ReadFromJsonAsync<List<AccountResponse>>(JsonOptions);
        accounts.Should().NotBeNull();
        accounts.Count.Should().Be(0);
    }

    [Fact]
    public async Task CreateAccount_WhenAuthenticated_ReturnsSuccess()
    {
        //Arrange
        var uniqueName = await AuthenticateAsync();

        //Act
        var response = await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == uniqueName);
        user.Should().NotBeNull();
        var record = await dbContext.Accounts.AnyAsync(x => x.UserId == user.Id);
        record.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAccount_WithoutToken_ReturnsUnauthorized()
    {
        //Arrange
        var client = Factory.CreateClient();

        //Act
        var response = await client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Deposit_ValidAmount_UpdatesBalanceAndRecordsTransaction()
    {
        //Arrange
        await AuthenticateAsync();
        var createAccountResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        createAccountResponse.EnsureSuccessStatusCode();
        var newAccount = await createAccountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        var accountId = newAccount!.Id;
        var amount = 1000;
        var transactionRequest = new TransactionRequest(accountId, amount);

        //Act
        var depositResponse = await Client.PostAsJsonAsync("/api/accounts/deposit", transactionRequest);

        //Assert
        depositResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
        account.Should().NotBeNull();
        account.Balance.Should().Be(amount);
        var transaction = await dbContext.Transactions.FirstOrDefaultAsync(t => t.AccountId == account.Id);
        transaction.Should().NotBeNull();
        transaction.Type.Should().Be(TransactionType.Deposit);
    }

    [Fact]
    public async Task Deposit_OtherUserAccount_ReturnsForbidden()
    {
        //Arrange
        await AuthenticateAsync();
        var ownerAccountResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        var ownerAccount = await ownerAccountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        var ownerAccountId = ownerAccount!.Id;
        await AuthenticateAsync();
        var amount = 1000;
        var transactionRequest = new TransactionRequest(ownerAccountId, amount);

        //Act
        var response = await Client.PostAsJsonAsync("/api/accounts/deposit", transactionRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == ownerAccountId);
        account.Should().NotBeNull();
        account.Balance.Should().Be(0);
    }

    [Fact]
    public async Task Deposit_NonExistentAccount_ReturnsNotFound()
    {
        //Arrange
        await AuthenticateAsync();
        var invalidAccountId = 999999;
        var amount = 1000;
        var transactionRequest = new TransactionRequest(invalidAccountId, amount);

        //Act
        var response = await Client.PostAsJsonAsync("/api/accounts/deposit", transactionRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Withdraw_ValidAmount_UpdatesBalanceAndRecordsTransaction()
    {
        //Arrange
        await AuthenticateAsync();
        var createAccountResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        createAccountResponse.EnsureSuccessStatusCode();
        var newAccount = await createAccountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        var accountId = newAccount!.Id;
        var depositAmount = 1000;
        var withdrawAmount = 500;
        var finalBalance = depositAmount - withdrawAmount;
        var depositTransactionRequest = new TransactionRequest(accountId, depositAmount);
        await Client.PostAsJsonAsync("/api/accounts/deposit", depositTransactionRequest);
        var withdrawTransactionRequest = new TransactionRequest(accountId, withdrawAmount);

        //Act
        var withdrawResponse = await Client.PostAsJsonAsync("/api/accounts/withdraw", withdrawTransactionRequest);

        //Assert
        withdrawResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
        account.Should().NotBeNull();
        account.Balance.Should().Be(finalBalance);
        var transaction = await dbContext.Transactions
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(t => t.AccountId == account.Id);
        transaction.Should().NotBeNull();
        transaction.Type.Should().Be(TransactionType.Withdraw);
    }

    [Fact]
    public async Task Withdraw_InsufficientFunds_ReturnsBadRequest()
    {
        //Arrange
        await AuthenticateAsync();
        var createAccountResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        createAccountResponse.EnsureSuccessStatusCode();
        var newAccount = await createAccountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        var accountId = newAccount!.Id;
        var depositAmount = 100;
        var withdrawAmount = 500;
        var depositTransactionRequest = new TransactionRequest(accountId, depositAmount);
        await Client.PostAsJsonAsync("/api/accounts/deposit", depositTransactionRequest);
        var withdrawTransactionRequest = new TransactionRequest(accountId, withdrawAmount);

        //Act
        var withdrawResponse = await Client.PostAsJsonAsync("/api/accounts/withdraw", withdrawTransactionRequest);

        //Assert
        withdrawResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
        account.Should().NotBeNull();
        account.Balance.Should().Be(depositAmount);
    }

    [Fact]
    public async Task Withdraw_OtherUserAccount_ReturnsForbidden()
    {
        //Arrange
        await AuthenticateAsync();
        var ownerAccountResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        var ownerAccount = await ownerAccountResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        var ownerAccountId = ownerAccount!.Id;
        await AuthenticateAsync();
        var amount = 1000;
        var transactionRequest = new TransactionRequest(ownerAccountId, amount);

        //Act
        var response = await Client.PostAsJsonAsync("/api/accounts/withdraw", transactionRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == ownerAccountId);
        account.Should().NotBeNull();
        account.Balance.Should().Be(0);
    }

    [Fact]
    public async Task Withdraw_NonExistentAccount_ReturnsNotFound()
    {
        //Arrange
        await AuthenticateAsync();
        var invalidAccountId = 999999;
        var amount = 1000;
        var transactionRequest = new TransactionRequest(invalidAccountId, amount);

        //Act
        var response = await Client.PostAsJsonAsync("/api/accounts/withdraw", transactionRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Transfer_ValidAmount_UpdatesBothBalancesAndRecordsTransactions()
    {
        //Arrange
        await AuthenticateAsync();
        var firstAccountCreateResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        firstAccountCreateResponse.EnsureSuccessStatusCode();
        var secondAccountCreateResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        secondAccountCreateResponse.EnsureSuccessStatusCode();
        var firstAccount = await firstAccountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        var secondAccount = await secondAccountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        var depositTransactionRequest = new TransactionRequest(firstAccount!.Id, 1000);
        await Client.PostAsJsonAsync("/api/accounts/deposit", depositTransactionRequest);
        var transferRequest = new TransferRequest(firstAccount.Id, secondAccount!.Id, 500);

        //Act
        var response = await Client.PostAsJsonAsync("/api/accounts/transfer", transferRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var firstAccountFromDb = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == firstAccount.Id);
        firstAccountFromDb.Should().NotBeNull();
        var secondAccountFromDb = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == secondAccount.Id);
        secondAccountFromDb.Should().NotBeNull();
        firstAccountFromDb.Balance.Should().Be(500);
        secondAccountFromDb.Balance.Should().Be(500);
        var fromAccountTransaction = await dbContext.Transactions
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(t => t.AccountId == firstAccountFromDb.Id);
        fromAccountTransaction.Should().NotBeNull();
        fromAccountTransaction.Amount.Should().Be(500);
        fromAccountTransaction.Type.Should().Be(TransactionType.Withdraw);
        var toAccountTransaction = await dbContext.Transactions
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(t => t.AccountId == secondAccountFromDb.Id);
        toAccountTransaction.Should().NotBeNull();
        toAccountTransaction.Amount.Should().Be(500);
        toAccountTransaction.Type.Should().Be(TransactionType.Deposit);
    }

    [Fact]
    public async Task Transfer_InsufficientFunds_ReturnsBadRequest()
    {
        //Arrange
        await AuthenticateAsync();
        var firstAccountCreateResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        firstAccountCreateResponse.EnsureSuccessStatusCode();
        var secondAccountCreateResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        secondAccountCreateResponse.EnsureSuccessStatusCode();
        var firstAccount = await firstAccountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        var secondAccount = await secondAccountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        var depositTransactionRequest = new TransactionRequest(firstAccount!.Id, 100);
        await Client.PostAsJsonAsync("/api/accounts/deposit", depositTransactionRequest);
        var transferRequest = new TransferRequest(firstAccount.Id, secondAccount!.Id, 500);

        //Act
        var response = await Client.PostAsJsonAsync("/api/accounts/transfer", transferRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var firstAccountFromDb = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == firstAccount.Id);
        firstAccountFromDb.Should().NotBeNull();
        var secondAccountFromDb = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == secondAccount.Id);
        secondAccountFromDb.Should().NotBeNull();
        firstAccountFromDb.Balance.Should().Be(100);
        secondAccountFromDb.Balance.Should().Be(0);
    }

    [Fact]
    public async Task Transfer_OtherUserAccountSender_ReturnsForbidden()
    {
        //Arrange
        await AuthenticateAsync();
        var ownerAccountCreateResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        var ownerAccount = await ownerAccountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        var depositOwnerAccountRequest = new TransactionRequest(ownerAccount!.Id, 1000);
        await Client.PostAsJsonAsync("/api/accounts/deposit", depositOwnerAccountRequest);
        await AuthenticateAsync();

        var invalidAccountId = 999999;
        var transferRequest = new TransferRequest(ownerAccount.Id, invalidAccountId, 500);

        //Act
        var response = await Client.PostAsJsonAsync("/api/accounts/transfer", transferRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ownerAccountFromDb = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == ownerAccount.Id);
        ownerAccountFromDb.Should().NotBeNull();
        ownerAccountFromDb.Balance.Should().Be(1000);
    }

    [Fact]
    public async Task Transfer_ToNonExistentAccount_ReturnsNotFound()
    {
        //Arrange
        await AuthenticateAsync();
        var accountCreateResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        accountCreateResponse.EnsureSuccessStatusCode();
        var account = await accountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        var depositTransactionRequest = new TransactionRequest(account!.Id, 1000);
        await Client.PostAsJsonAsync("/api/accounts/deposit", depositTransactionRequest);
        var nonExistentAccountId = 999999;
        var transferRequest = new TransferRequest(account.Id, nonExistentAccountId, 500);

        //Act
        var response = await Client.PostAsJsonAsync("/api/accounts/transfer", transferRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAccountTransactions_WhenAuthenticated_ReturnsSuccessAndTransactions()
    {
        //Arrange
        await AuthenticateAsync();
        var accountCreateResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        accountCreateResponse.EnsureSuccessStatusCode();
        var account = await accountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        var depositTransactionRequest = new TransactionRequest(account!.Id, 1000);
        var withdrawTransactionRequest = new TransactionRequest(account.Id, 500);
        await Client.PostAsJsonAsync("/api/accounts/deposit", depositTransactionRequest);
        await Client.PostAsJsonAsync("/api/accounts/withdraw", withdrawTransactionRequest);

        //Act
        var response = await Client.GetAsync($"/api/accounts/{account.Id}/transactions");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var transactions = await response.Content.ReadFromJsonAsync<List<Transaction>>(JsonOptions);
        transactions.Should().NotBeNull();
        transactions.Count.Should().Be(2);
        transactions[1].Amount.Should().Be(1000);
        transactions[1].Type.Should().Be(TransactionType.Deposit);
        transactions[0].Amount.Should().Be(500);
        transactions[0].Type.Should().Be(TransactionType.Withdraw);
    }

    [Fact]
    public async Task GetAccountTransactions_OtherUserAccount_ReturnsForbidden()
    {
        //Arrange
        await AuthenticateAsync();
        var ownerAccountCreateResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        var ownerAccount = await ownerAccountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        var depositOwnerAccountRequest = new TransactionRequest(ownerAccount!.Id, 1000);
        await Client.PostAsJsonAsync("/api/accounts/deposit", depositOwnerAccountRequest);
        await AuthenticateAsync();

        //Act
        var response = await Client.GetAsync($"/api/accounts/{ownerAccount.Id}/transactions");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteAccount_WhenAuthenticated_ReturnsSuccessAndHidesAccount()
    {
        //Arrange
        await AuthenticateAsync();
        var accountCreateResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        accountCreateResponse.EnsureSuccessStatusCode();
        var account = await accountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);

        //Act
        var response = await Client.DeleteAsync($"/api/accounts/{account!.Id}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var deletedAccount = await dbContext.Accounts.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == account.Id);
        deletedAccount.Should().NotBeNull();
        deletedAccount.IsDeleted.Should().Be(true);
        var accountsResponse = await Client.GetAsync("/api/accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<AccountResponse>>(JsonOptions);
        accounts.Should().NotBeNull();
        accounts.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteAccount_OtherUserAccount_ReturnsForbidden()
    {
        //Arrange
        await AuthenticateAsync();
        var ownerAccountCreateResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        var ownerAccount = await ownerAccountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        await AuthenticateAsync();

        //Act
        var response = await Client.DeleteAsync($"/api/accounts/{ownerAccount!.Id}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var deletedAccount = await dbContext.Accounts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == ownerAccount.Id);
        deletedAccount.Should().NotBeNull();
        deletedAccount.IsDeleted.Should().Be(false);
    }

    [Fact]
    public async Task Delete_NotExistentAccount_ReturnsNotFound()
    {
        //Arrange
        await AuthenticateAsync();
        var accountCreateResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current));
        accountCreateResponse.EnsureSuccessStatusCode();
        await accountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        var nonExistentAccountId = 999999;

        //Act
        var response = await Client.DeleteAsync($"/api/accounts/{nonExistentAccountId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}