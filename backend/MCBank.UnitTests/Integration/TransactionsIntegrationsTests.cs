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

public class TransactionsIntegrationsTests(MCBankApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Deposit_ValidAmount_UpdatesBalanceAndRecordsTransaction()
    {
        //Arrange
        await AuthenticateAsync();
        var account = await CreateAccountAsync(AccountType.Current);
        var amount = 1000;

        //Act
        var depositResponse = await PostTransactionAsync("deposit", account.Id, amount);

        //Assert
        depositResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var accountDb = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);
        accountDb.Should().NotBeNull();
        accountDb.Balance.Should().Be(amount);
        var transaction = await dbContext.Transactions.FirstOrDefaultAsync(t => t.AccountId == accountDb.Id);
        transaction.Should().NotBeNull();   
        transaction.Type.Should().Be(TransactionType.Deposit);
    }

    [Fact]
    public async Task Deposit_OtherUserAccount_ReturnsForbidden()
    {
        //Arrange
        await AuthenticateAsync();
        var ownerAccount = await CreateAccountAsync(AccountType.Current);
        await AuthenticateAsync();
        var amount = 1000;

        //Act
        var response = await PostTransactionAsync("deposit", ownerAccount.Id, amount);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == ownerAccount.Id);
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

        //Act
        var response = await PostTransactionAsync("deposit", invalidAccountId, amount);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Withdraw_ValidAmount_UpdatesBalanceAndRecordsTransaction()
    {
        //Arrange
        await AuthenticateAsync();
        var account = await CreateAccountAsync(AccountType.Current);
        var depositAmount = 1000;
        await PostTransactionAsync("deposit", account.Id, depositAmount);
        var withdrawAmount = 500;
        var finalBalance = depositAmount - withdrawAmount;

        //Act
        var withdrawResponse = await PostTransactionAsync("withdraw", account.Id, withdrawAmount);

        //Assert
        withdrawResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var accountDb = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);
        accountDb.Should().NotBeNull();
        accountDb.Balance.Should().Be(finalBalance);
        var transaction = await dbContext.Transactions
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(t => t.AccountId == accountDb.Id);
        transaction.Should().NotBeNull();
        transaction.Type.Should().Be(TransactionType.Withdraw);
    }

    [Fact]
    public async Task Withdraw_InsufficientFunds_ReturnsBadRequest()
    {
        //Arrange
        await AuthenticateAsync();
        var account = await CreateAccountAsync(AccountType.Current);
        var depositAmount = 100;
        await PostTransactionAsync("deposit", account.Id, depositAmount);
        var withdrawAmount = 500;

        //Act
        var withdrawResponse = await PostTransactionAsync("withdraw", account.Id, withdrawAmount);

        //Assert
        withdrawResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var accountDb = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);
        accountDb.Should().NotBeNull();
        accountDb.Balance.Should().Be(depositAmount);
    }

    [Fact]
    public async Task Withdraw_OtherUserAccount_ReturnsForbidden()
    {
        //Arrange
        await AuthenticateAsync();
        var ownerAccount = await CreateAccountAsync(AccountType.Current);
        await AuthenticateAsync();
        var amount = 1000;

        //Act
        var response = await PostTransactionAsync("withdraw", ownerAccount.Id, amount);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == ownerAccount.Id);
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

        //Act
        var response = await PostTransactionAsync("withdraw", invalidAccountId, amount);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Transfer_ValidAmount_UpdatesBothBalancesAndRecordsTransactions()
    {
        //Arrange
        await AuthenticateAsync();
        var firstAccount = await CreateAccountAsync(AccountType.Current);
        var secondAccount = await CreateAccountAsync(AccountType.Current);
        await PostTransactionAsync("deposit", firstAccount.Id, 1000);
        var transferRequest = new TransferRequest(firstAccount.Id, secondAccount.Id, 500);

        //Act
        var response = await Client.PostAsJsonAsync("/api/transactions/transfer", transferRequest);

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
        var firstAccount = await CreateAccountAsync(AccountType.Current);
        var secondAccount = await CreateAccountAsync(AccountType.Current);
        await PostTransactionAsync("deposit", firstAccount.Id, 100);
        var transferRequest = new TransferRequest(firstAccount.Id, secondAccount.Id, 500);

        //Act
        var response = await Client.PostAsJsonAsync("/api/transactions/transfer", transferRequest);

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
        var ownerAccount = await CreateAccountAsync(AccountType.Current);
        await PostTransactionAsync("deposit", ownerAccount.Id, 1000);
        await AuthenticateAsync();
        var invalidAccountId = 999999;
        var transferRequest = new TransferRequest(ownerAccount.Id, invalidAccountId, 500);

        //Act
        var response = await Client.PostAsJsonAsync("/api/transactions/transfer", transferRequest);

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
        var account = await CreateAccountAsync(AccountType.Current);
        await PostTransactionAsync("deposit", account.Id, 1000);
        var nonExistentAccountId = 999999;
        var transferRequest = new TransferRequest(account.Id, nonExistentAccountId, 500);

        //Act
        var response = await Client.PostAsJsonAsync("/api/transactions/transfer", transferRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAccountTransactions_WhenAuthenticated_ReturnsSuccessAndTransactions()
    {
        //Arrange
        await AuthenticateAsync();
        var account = await CreateAccountAsync(AccountType.Current);
        await PostTransactionAsync("deposit", account.Id, 1000);
        await PostTransactionAsync("withdraw", account.Id, 500);

        //Act
        var response = await Client.GetAsync($"/api/transactions/{account.Id}");

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
        var ownerAccount = await CreateAccountAsync(AccountType.Current);
        await PostTransactionAsync("deposit", ownerAccount.Id, 1000);
        await AuthenticateAsync();

        //Act
        var response = await Client.GetAsync($"/api/transactions/{ownerAccount.Id}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}