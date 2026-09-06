using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Core.Entities;
using MCBank.WebApi.Core.Enums;
using MCBank.WebApi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MCBank.UnitTests.Integration;

public class AccountsIntegrationTests(MCBankApiFactory factory) : IClassFixture<MCBankApiFactory>
{
    [Fact]
    public async Task GetAccountById_WhenAuthenticated_ReturnsSuccess()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);
        registerResponse.EnsureSuccessStatusCode();
        var tokenPairDto = await registerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var token = tokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var accountResponse = await client.PostAsync("/api/accounts", null);
        accountResponse.EnsureSuccessStatusCode();
        var createdAccount = await accountResponse.Content.ReadFromJsonAsync<AccountResponse>();
        createdAccount.Should().NotBeNull();

        //Act
        var getAccountResponse = await client.GetAsync($"/api/accounts/{createdAccount.Id}");

        //Assert
        getAccountResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var account = await getAccountResponse.Content.ReadFromJsonAsync<AccountResponse>();
        account.Should().NotBeNull();
        account.Id.Should().Be(createdAccount.Id);
        account.Iban.Should().Be(createdAccount.Iban);
    }

    [Fact]
    public async Task GetAccountById_OtherUserAccount_ReturnsForbidden()
    {
        //Arrange
        var client = factory.CreateClient();
        var ownerUserName = $"user_{Guid.NewGuid()}";
        var owner = new RegisterRequest { Username = ownerUserName, Password = "password" };
        var ownerRegisterResponse = await client.PostAsJsonAsync("/api/auth/register", owner);
        var ownerTokenPairDto = await ownerRegisterResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var ownerAccessToken = ownerTokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerAccessToken);
        var ownerAccountResponse = await client.PostAsync("/api/accounts", null);
        ownerAccountResponse.EnsureSuccessStatusCode();
        var ownerAccount = await ownerAccountResponse.Content.ReadFromJsonAsync<AccountResponse>();
        ownerAccount.Should().NotBeNull();
        var ownerAccountId = ownerAccount.Id;
        var otherUserName = $"user_{Guid.NewGuid()}";
        var otherUser = new RegisterRequest { Username = otherUserName, Password = "password" };
        var otherUserRegisterResponse = await client.PostAsJsonAsync("/api/auth/register", otherUser);
        var otherUserTokenPairDto = await otherUserRegisterResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var otherUserAccessToken = otherUserTokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherUserAccessToken);

        //Act
        var response = await client.GetAsync($"/api/accounts/{ownerAccountId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAccounts_WhenAuthenticated_ReturnsSuccessAndAllUserAccounts()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);
        registerResponse.EnsureSuccessStatusCode();
        var tokenPairDto = await registerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var token = tokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await client.PostAsync("/api/accounts", null);
        await client.PostAsync("/api/accounts", null);
        await client.PostAsync("/api/accounts", null);

        //Act
        var response = await client.GetAsync("/api/accounts");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var accountsCount = await dbContext.Accounts.CountAsync(u => u.User.Username == uniqueName);
        accountsCount.Should().Be(3);

        var accounts = await response.Content.ReadFromJsonAsync<List<AccountResponse>>();
        accounts.Should().NotBeNull();
        accounts.Count.Should().Be(accountsCount);
    }

    [Fact]
    public async Task GetAccounts_ShouldOnlyReturnOwnAccounts()
    {
        //Arrange
        var client = factory.CreateClient();
        var ownerUserName = $"user_{Guid.NewGuid()}";
        var owner = new RegisterRequest { Username = ownerUserName, Password = "password" };
        var registerOwnerResponse = await client.PostAsJsonAsync("/api/auth/register", owner);
        registerOwnerResponse.EnsureSuccessStatusCode();
        var ownerTokenPairDto = await registerOwnerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var ownerToken = ownerTokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        await client.PostAsync("/api/accounts", null);
        await client.PostAsync("/api/accounts", null);
        await client.PostAsync("/api/accounts", null);

        var otherUserName = $"user_{Guid.NewGuid()}";
        var otherUser = new RegisterRequest { Username = otherUserName, Password = "password" };
        var registerOtherUserResponse = await client.PostAsJsonAsync("/api/auth/register", otherUser);
        registerOtherUserResponse.EnsureSuccessStatusCode();
        var otherUserTokenPairDto = await registerOtherUserResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var otherUserToken = otherUserTokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherUserToken);

        //Act
        var response = await client.GetAsync("/api/accounts");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var accounts = await response.Content.ReadFromJsonAsync<List<AccountResponse>>();
        accounts.Should().NotBeNull();
        accounts.Count.Should().Be(0);
    }

    [Fact]
    public async Task CreateAccount_WithoutToken_ReturnsUnauthorized()
    {
        //Arrange
        var client = factory.CreateClient();

        //Act
        var response = await client.PostAsync("/api/accounts", null);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Deposit_ValidAmount_UpdatesBalanceAndRecordsTransaction()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);
        registerResponse.EnsureSuccessStatusCode();
        var tokenPairDto = await registerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var token = tokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var createAccountResponse = await client.PostAsync("/api/accounts", null);
        createAccountResponse.EnsureSuccessStatusCode();
        var newAccount = await createAccountResponse.Content.ReadFromJsonAsync<AccountResponse>();
        var accountId = newAccount!.Id;
        var amount = 1000;
        var transactionRequest = new TransactionRequest(accountId, amount);

        //Act
        var depositResponse = await client.PostAsJsonAsync("/api/accounts/deposit", transactionRequest);

        //Assert
        depositResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = factory.Services.CreateScope();
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
        var client = factory.CreateClient();
        var ownerUserName = $"user_{Guid.NewGuid()}";
        var owner = new RegisterRequest { Username = ownerUserName, Password = "password" };
        var registerOwnerResponse = await client.PostAsJsonAsync("/api/auth/register", owner);
        registerOwnerResponse.EnsureSuccessStatusCode();
        var ownerTokenPairDto = await registerOwnerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var ownerToken = ownerTokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        var ownerAccountResponse = await client.PostAsync("/api/accounts", null);
        var ownerAccount = await ownerAccountResponse.Content.ReadFromJsonAsync<AccountResponse>();
        var ownerAccountId = ownerAccount!.Id;

        var otherUserName = $"user_{Guid.NewGuid()}";
        var otherUser = new RegisterRequest { Username = otherUserName, Password = "password" };
        var registerOtherUserResponse = await client.PostAsJsonAsync("/api/auth/register", otherUser);
        registerOtherUserResponse.EnsureSuccessStatusCode();
        var otherUserTokenPairDto = await registerOtherUserResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var otherUserToken = otherUserTokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherUserToken);

        var amount = 1000;
        var transactionRequest = new TransactionRequest(ownerAccountId, amount);

        //Act
        var response = await client.PostAsJsonAsync("/api/accounts/deposit", transactionRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == ownerAccountId);
        account.Should().NotBeNull();
        account.Balance.Should().Be(0);
    }

    [Fact]
    public async Task Deposit_NonExistentAccount_ReturnsNotFound()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);
        registerResponse.EnsureSuccessStatusCode();
        var tokenPairDto = await registerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var token = tokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var invalidAccountId = 999999;
        var amount = 1000;
        var transactionRequest = new TransactionRequest(invalidAccountId, amount);

        //Act
        var response = await client.PostAsJsonAsync("/api/accounts/deposit", transactionRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Withdraw_ValidAmount_UpdatesBalanceAndRecordsTransaction()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);
        registerResponse.EnsureSuccessStatusCode();
        var tokenPairDto = await registerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var token = tokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var createAccountResponse = await client.PostAsync("/api/accounts", null);
        createAccountResponse.EnsureSuccessStatusCode();
        var newAccount = await createAccountResponse.Content.ReadFromJsonAsync<AccountResponse>();
        var accountId = newAccount!.Id;
        var depositAmount = 1000;
        var withdrawAmount = 500;
        var finalBalance = depositAmount - withdrawAmount;
        var depositTransactionRequest = new TransactionRequest(accountId, depositAmount);
        await client.PostAsJsonAsync("/api/accounts/deposit", depositTransactionRequest);
        var withdrawTransactionRequest = new TransactionRequest(accountId, withdrawAmount);

        //Act
        var withdrawResponse = await client.PostAsJsonAsync("/api/accounts/withdraw", withdrawTransactionRequest);

        //Assert
        withdrawResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = factory.Services.CreateScope();
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
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);
        registerResponse.EnsureSuccessStatusCode();
        var tokenPairDto = await registerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var token = tokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var createAccountResponse = await client.PostAsync("/api/accounts", null);
        createAccountResponse.EnsureSuccessStatusCode();
        var newAccount = await createAccountResponse.Content.ReadFromJsonAsync<AccountResponse>();
        var accountId = newAccount!.Id;
        var depositAmount = 100;
        var withdrawAmount = 500;
        var depositTransactionRequest = new TransactionRequest(accountId, depositAmount);
        await client.PostAsJsonAsync("/api/accounts/deposit", depositTransactionRequest);
        var withdrawTransactionRequest = new TransactionRequest(accountId, withdrawAmount);

        //Act
        var withdrawResponse = await client.PostAsJsonAsync("/api/accounts/withdraw", withdrawTransactionRequest);

        //Assert
        withdrawResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
        account.Should().NotBeNull();
        account.Balance.Should().Be(depositAmount);
    }

    [Fact]
    public async Task Withdraw_OtherUserAccount_ReturnsForbidden()
    {
        //Arrange
        var client = factory.CreateClient();
        var ownerUserName = $"user_{Guid.NewGuid()}";
        var owner = new RegisterRequest { Username = ownerUserName, Password = "password" };
        var registerOwnerResponse = await client.PostAsJsonAsync("/api/auth/register", owner);
        registerOwnerResponse.EnsureSuccessStatusCode();
        var ownerTokenPairDto = await registerOwnerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var ownerToken = ownerTokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        var ownerAccountResponse = await client.PostAsync("/api/accounts", null);
        var ownerAccount = await ownerAccountResponse.Content.ReadFromJsonAsync<AccountResponse>();
        var ownerAccountId = ownerAccount!.Id;

        var otherUserName = $"user_{Guid.NewGuid()}";
        var otherUser = new RegisterRequest { Username = otherUserName, Password = "password" };
        var registerOtherUserResponse = await client.PostAsJsonAsync("/api/auth/register", otherUser);
        registerOtherUserResponse.EnsureSuccessStatusCode();
        var otherUserTokenPairDto = await registerOtherUserResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var otherUserToken = otherUserTokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherUserToken);

        var amount = 1000;
        var transactionRequest = new TransactionRequest(ownerAccountId, amount);

        //Act
        var response = await client.PostAsJsonAsync("/api/accounts/withdraw", transactionRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == ownerAccountId);
        account.Should().NotBeNull();
        account.Balance.Should().Be(0);
    }

    [Fact]
    public async Task Withdraw_NonExistentAccount_ReturnsNotFound()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);
        registerResponse.EnsureSuccessStatusCode();
        var tokenPairDto = await registerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var token = tokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var invalidAccountId = 999999;
        var amount = 1000;
        var transactionRequest = new TransactionRequest(invalidAccountId, amount);

        //Act
        var response = await client.PostAsJsonAsync("/api/accounts/withdraw", transactionRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Transfer_ValidAmount_UpdatesBothBalancesAndRecordsTransactions()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);
        registerResponse.EnsureSuccessStatusCode();
        var tokenPairDto = await registerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var token = tokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var firstAccountCreateResponse = await client.PostAsync("/api/accounts", null);
        firstAccountCreateResponse.EnsureSuccessStatusCode();
        var secondAccountCreateResponse = await client.PostAsync("/api/accounts", null);
        secondAccountCreateResponse.EnsureSuccessStatusCode();
        var firstAccount = await firstAccountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>();
        var secondAccount = await secondAccountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>();
        var depositTransactionRequest = new TransactionRequest(firstAccount!.Id, 1000);
        await client.PostAsJsonAsync("/api/accounts/deposit", depositTransactionRequest);
        var transferRequest = new TransferRequest(firstAccount.Id, secondAccount!.Id, 500);

        //Act
        var response = await client.PostAsJsonAsync("/api/accounts/transfer", transferRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = factory.Services.CreateScope();
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
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);
        registerResponse.EnsureSuccessStatusCode();
        var tokenPairDto = await registerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var token = tokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var firstAccountCreateResponse = await client.PostAsync("/api/accounts", null);
        firstAccountCreateResponse.EnsureSuccessStatusCode();
        var secondAccountCreateResponse = await client.PostAsync("/api/accounts", null);
        secondAccountCreateResponse.EnsureSuccessStatusCode();
        var firstAccount = await firstAccountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>();
        var secondAccount = await secondAccountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>();
        var depositTransactionRequest = new TransactionRequest(firstAccount!.Id, 100);
        await client.PostAsJsonAsync("/api/accounts/deposit", depositTransactionRequest);
        var transferRequest = new TransferRequest(firstAccount.Id, secondAccount!.Id, 500);

        //Act
        var response = await client.PostAsJsonAsync("/api/accounts/transfer", transferRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var scope = factory.Services.CreateScope();
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
        var client = factory.CreateClient();
        var uniqueOwnerName = $"user_{Guid.NewGuid()}";
        var ownerRegisterRequest = new RegisterRequest { Username = uniqueOwnerName, Password = "password" };
        var registerOwnerResponse = await client.PostAsJsonAsync("/api/auth/register", ownerRegisterRequest);
        registerOwnerResponse.EnsureSuccessStatusCode();
        var ownerTokenPairDto = await registerOwnerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var ownerAccessToken = ownerTokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerAccessToken);
        var ownerAccountCreateResponse = await client.PostAsync("/api/accounts", null);
        var ownerAccount = await ownerAccountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>();
        var depositOwnerAccountRequest = new TransactionRequest(ownerAccount!.Id, 1000);
        await client.PostAsJsonAsync("/api/accounts/deposit", depositOwnerAccountRequest);

        var otherUserUniqueName = $"user_{Guid.NewGuid()}";
        var otherUserRegisterRequest = new RegisterRequest { Username = otherUserUniqueName, Password = "password" };
        var registerOtherUserResponse = await client.PostAsJsonAsync("/api/auth/register", otherUserRegisterRequest);
        registerOtherUserResponse.EnsureSuccessStatusCode();
        var otherUserTokenPairDto = await registerOtherUserResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var otherUserAccessToken = otherUserTokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherUserAccessToken);
        var invalidAccountId = 999999;

        var transferRequest = new TransferRequest(ownerAccount.Id, invalidAccountId, 500);

        //Act
        var response = await client.PostAsJsonAsync("/api/accounts/transfer", transferRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ownerAccountFromDb = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == ownerAccount.Id);
        ownerAccountFromDb.Should().NotBeNull();
        ownerAccountFromDb.Balance.Should().Be(1000);
    }

    [Fact]
    public async Task Transfer_ToNonExistentAccount_ReturnsNotFound()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);
        registerResponse.EnsureSuccessStatusCode();
        var tokenPairDto = await registerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var token = tokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var accountCreateResponse = await client.PostAsync("/api/accounts", null);
        accountCreateResponse.EnsureSuccessStatusCode();

        var account = await accountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>();
        var depositTransactionRequest = new TransactionRequest(account!.Id, 1000);
        await client.PostAsJsonAsync("/api/accounts/deposit", depositTransactionRequest);
        var nonExistentAccountId = 999999;
        var transferRequest = new TransferRequest(account.Id, nonExistentAccountId, 500);

        //Act
        var response = await client.PostAsJsonAsync("/api/accounts/transfer", transferRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAccountTransactions_WhenAuthenticated_ReturnsSuccessAndTransactions()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);
        registerResponse.EnsureSuccessStatusCode();
        var tokenPairDto = await registerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var token = tokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var accountCreateResponse = await client.PostAsync("/api/accounts", null);
        accountCreateResponse.EnsureSuccessStatusCode();
        var account = await accountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>();
        var depositTransactionRequest = new TransactionRequest(account!.Id, 1000);
        var withdrawTransactionRequest = new TransactionRequest(account!.Id, 500);
        await client.PostAsJsonAsync("/api/accounts/deposit", depositTransactionRequest);
        await client.PostAsJsonAsync("/api/accounts/withdraw", withdrawTransactionRequest);

        //Act
        var response = await client.GetAsync($"/api/accounts/{account.Id}/transactions");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var transactions = await response.Content.ReadFromJsonAsync<List<Transaction>>();
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
        var client = factory.CreateClient();
        var uniqueOwnerName = $"user_{Guid.NewGuid()}";
        var ownerRegisterRequest = new RegisterRequest { Username = uniqueOwnerName, Password = "password" };
        var registerOwnerResponse = await client.PostAsJsonAsync("/api/auth/register", ownerRegisterRequest);
        registerOwnerResponse.EnsureSuccessStatusCode();
        var ownerTokenPairDto = await registerOwnerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var ownerAccessToken = ownerTokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerAccessToken);
        var ownerAccountCreateResponse = await client.PostAsync("/api/accounts", null);
        var ownerAccount = await ownerAccountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>();
        var depositOwnerAccountRequest = new TransactionRequest(ownerAccount!.Id, 1000);
        await client.PostAsJsonAsync("/api/accounts/deposit", depositOwnerAccountRequest);

        var otherUserUniqueName = $"user_{Guid.NewGuid()}";
        var otherUserRegisterRequest = new RegisterRequest { Username = otherUserUniqueName, Password = "password" };
        var registerOtherUserResponse = await client.PostAsJsonAsync("/api/auth/register", otherUserRegisterRequest);
        registerOtherUserResponse.EnsureSuccessStatusCode();
        var otherUserTokenPairDto = await registerOtherUserResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var otherUserAccessToken = otherUserTokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherUserAccessToken);

        //Act
        var response = await client.GetAsync($"/api/accounts/{ownerAccount.Id}/transactions");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteAccount_WhenAuthenticated_ReturnsSuccessAndHidesAccount()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);
        registerResponse.EnsureSuccessStatusCode();
        var tokenPairDto = await registerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var token = tokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var accountCreateResponse = await client.PostAsync("/api/accounts", null);
        accountCreateResponse.EnsureSuccessStatusCode();
        var account = await accountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>();

        //Act
        var response = await client.DeleteAsync($"/api/accounts/{account!.Id}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var deletedAccount = await dbContext.Accounts.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == account.Id);
        deletedAccount.Should().NotBeNull();
        deletedAccount.IsDeleted.Should().Be(true);
        var accountsResponse = await client.GetAsync("/api/accounts");
        var accounts = await accountsResponse.Content.ReadFromJsonAsync<List<AccountResponse>>();
        accounts.Should().NotBeNull();
        accounts.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteAccount_OtherUserAccount_ReturnsForbidden()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueOwnerName = $"user_{Guid.NewGuid()}";
        var ownerRegisterRequest = new RegisterRequest { Username = uniqueOwnerName, Password = "password" };
        var registerOwnerResponse = await client.PostAsJsonAsync("/api/auth/register", ownerRegisterRequest);
        registerOwnerResponse.EnsureSuccessStatusCode();
        var ownerTokenPairDto = await registerOwnerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var ownerAccessToken = ownerTokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerAccessToken);
        var ownerAccountCreateResponse = await client.PostAsync("/api/accounts", null);
        var ownerAccount = await ownerAccountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>();

        var otherUserUniqueName = $"user_{Guid.NewGuid()}";
        var otherUserRegisterRequest = new RegisterRequest { Username = otherUserUniqueName, Password = "password" };
        var registerOtherUserResponse = await client.PostAsJsonAsync("/api/auth/register", otherUserRegisterRequest);
        registerOtherUserResponse.EnsureSuccessStatusCode();
        var otherUserTokenPairDto = await registerOtherUserResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var otherUserAccessToken = otherUserTokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherUserAccessToken);

        //Act
        var response = await client.DeleteAsync($"/api/accounts/{ownerAccount!.Id}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var scope = factory.Services.CreateScope();
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
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);
        registerResponse.EnsureSuccessStatusCode();
        var tokenPairDto = await registerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var token = tokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var accountCreateResponse = await client.PostAsync("/api/accounts", null);
        accountCreateResponse.EnsureSuccessStatusCode();
        await accountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>();
        var nonExistentAccountId = 999999;

        //Act
        var response = await client.DeleteAsync($"/api/accounts/{nonExistentAccountId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAccount_WhenAuthenticated_ReturnsSuccess()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);
        registerResponse.EnsureSuccessStatusCode();
        var tokenPairDto = await registerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        var token = tokenPairDto!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //Act
        var response = await client.PostAsync("/api/accounts", null);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == uniqueName);
        user.Should().NotBeNull();
        var record = await dbContext.Accounts.AnyAsync(x => x.UserId == user.Id);
        record.Should().BeTrue();
    }
}