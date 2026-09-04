using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MCBank.UnitTests.Integration;

public class AuthorizationIntegrationTests(MCBankApiFactory factory) : IClassFixture<MCBankApiFactory>
{
    [Fact]
    public async Task Register_WithValidData_ReturnsOkAndTokens()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };

        //Act
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);

        //Assert
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tokenPairDto = await registerResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        tokenPairDto.Should().NotBeNull();
        tokenPairDto.AccessToken.Should().NotBeNullOrWhiteSpace();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == uniqueName);
        user.Should().NotBeNull();
        user.Username.Should().Be(uniqueName);
        user.PasswordHash.Should().NotBe(newUser.Password);
    }

    [Fact]
    public async Task Register_WithExistingUsername_ReturnsConflict()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        await client.PostAsJsonAsync("/api/auth/register", newUser);

        //Act
        var secondRegisterResponse = await client.PostAsJsonAsync("/api/auth/register", newUser);

        //Assert
        secondRegisterResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var usersCount = await dbContext.Users.CountAsync(u => u.Username == uniqueName);
        usersCount.Should().Be(1);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkAndTokens()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var password = "password";
        var newUser = new RegisterRequest { Username = uniqueName, Password = password };
        await client.PostAsJsonAsync("/api/auth/register", newUser);
        var loginRequest = new LoginRequest { Username = uniqueName, Password = password };

        //Act
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        //Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokenPairDto = await loginResponse.Content.ReadFromJsonAsync<TokenPairDto>();
        tokenPairDto.Should().NotBeNull();
        tokenPairDto.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        //Arrange
        var client = factory.CreateClient();
        var uniqueName = $"user_{Guid.NewGuid()}";
        var newUser = new RegisterRequest { Username = uniqueName, Password = "password" };
        await client.PostAsJsonAsync("/api/auth/register", newUser);
        var loginRequest = new LoginRequest { Username = uniqueName, Password = "invalidPassword" };

        //Act
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        //Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        //Arrange
        var client = factory.CreateClient();
        var loginRequest = new LoginRequest { Username = "invalidUsername", Password = "password" };

        //Act
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        //Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}