using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Core.Enums;

namespace MCBank.UnitTests.Integration;

public class IntegrationTestBase(MCBankApiFactory factory) : IClassFixture<MCBankApiFactory>
{
    protected readonly MCBankApiFactory Factory = factory;
    protected readonly HttpClient Client = factory.CreateClient();

    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    protected async Task<string> AuthenticateAsync()
    {
        var uniqueName = $"user_{Guid.NewGuid()}";
        var registerRequest = new RegisterRequest { Username = uniqueName, Password = "password" };
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);
        response.EnsureSuccessStatusCode();

        return uniqueName;
    }

    protected async Task<AccountResponse> CreateAccountAsync(AccountType type, int? termMonths = null)
    {
        var response = await Client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest(type, termMonths));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccountResponse>(JsonOptions))!;
    }

    protected async Task<HttpResponseMessage> PostTransactionAsync(string endpoint, int accountId, decimal amount) => 
        await Client.PostAsJsonAsync($"/api/transactions/{endpoint}", new TransactionRequest(accountId, amount));
}