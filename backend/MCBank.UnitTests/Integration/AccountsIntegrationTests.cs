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
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current, null));
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
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current, null));
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
        await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current, null));
        await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current, null));
        await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current, null));

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
        await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current, null));
        await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current, null));
        await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current, null));
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
    public async Task CreateAccount_Current_WhenAuthenticated_ReturnsSuccess()
    {
        //Arrange
        var uniqueName = await AuthenticateAsync();

        //Act
        var response =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current, null));

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
        var response =
            await client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current, null));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAccount_SavingsFixed_ReturnsSuccessWithCorrectData()
    {
        //Arrange
        await AuthenticateAsync();
        var termMonths = 3;

        //Act
        var response =
            await Client.PostAsJsonAsync("/api/accounts",
                new CreateAccountRequest(AccountType.SavingsFixed, termMonths));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var account = await response.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        account.Should().NotBeNull();
        account.Type.Should().Be(AccountType.SavingsFixed);
        account.InterestRate.Should().Be(18.0m);
        account.ExpirationDate.Should().NotBeNull();
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbAccount = await dbContext.Accounts.FindAsync(account.Id);
        dbAccount.Should().NotBeNull();
        dbAccount.ExpirationDate.Should().BeCloseTo(DateTime.UtcNow.AddMonths(termMonths), TimeSpan.FromMinutes(1));
    }

    public static IEnumerable<object[]> GetAccountData() =>
        new List<object[]>
        {
            new object[] { AccountType.Current, 0, null! },
            new object[] { AccountType.SavingsFlexible, 12, (decimal?)14.5 },
            new object[] { AccountType.SavingsFixed, 3, (decimal?)18.0 },
            new object[] { AccountType.SavingsFixed, 6, (decimal?)17.5 },
            new object[] { AccountType.SavingsFixed, 12, (decimal?)15.5 },
            new object[] { AccountType.SavingsFixed, 24, (decimal?)12.5 },
            new object[] { AccountType.SavingsReplenishable, 3, (decimal?)19.0 },
            new object[] { AccountType.SavingsReplenishable, 6, (decimal?)18.5 },
            new object[] { AccountType.SavingsReplenishable, 12, (decimal?)15.0 },
            new object[] { AccountType.SavingsReplenishable, 24, (decimal?)5.1 }
        };

    [Theory]
    [MemberData(nameof(GetAccountData))]
    public async Task CreateAccount_Theory(AccountType type, int termMonths, decimal? expectedRate)
    {
        //Arrange
        await AuthenticateAsync();
        var request = new CreateAccountRequest(type, termMonths);
        
        //Act
        var response = await Client.PostAsJsonAsync("/api/accounts", request);
        response.EnsureSuccessStatusCode();
        
        //Assert
        var account = await response.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        account.Should().NotBeNull();
        account.Type.Should().Be(type);
    
        if (expectedRate.HasValue)
        {
            account.InterestRate.Should().Be(expectedRate.Value);
            account.ExpirationDate.Should().NotBeNull();
        }
        else
        {
            account.InterestRate.Should().BeNull();
            account.ExpirationDate.Should().BeNull();
        }
    }

    [Fact]
    public async Task DeleteAccount_WhenAuthenticated_ReturnsSuccessAndHidesAccount()
    {
        //Arrange
        await AuthenticateAsync();
        var accountCreateResponse =
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current, null));
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
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current, null));
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
            await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(AccountType.Current, null));
        accountCreateResponse.EnsureSuccessStatusCode();
        await accountCreateResponse.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions);
        var nonExistentAccountId = 999999;

        //Act
        var response = await Client.DeleteAsync($"/api/accounts/{nonExistentAccountId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}