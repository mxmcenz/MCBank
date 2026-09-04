using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MCBank.WebApi.Application.DTOs;
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
    
    //Withdraw Tests

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