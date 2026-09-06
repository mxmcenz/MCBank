using System.Net.Http.Headers;
using System.Net.Http.Json;
using MCBank.WebApi.Application.DTOs;

namespace MCBank.UnitTests.Integration;

public class IntegrationTestBase(MCBankApiFactory factory) : IClassFixture<MCBankApiFactory>
{
    protected readonly MCBankApiFactory Factory = factory;
    protected readonly HttpClient Client = factory.CreateClient();

    protected async Task<string> AuthenticateAsync()
    {
        var uniqueName = $"user_{Guid.NewGuid()}";
        var registerRequest = new RegisterRequest { Username = uniqueName, Password = "password" };
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var tokenPairDto = await response.Content.ReadFromJsonAsync<TokenPairDto>();
        var accessToken = tokenPairDto!.AccessToken;
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return uniqueName;
    }
}