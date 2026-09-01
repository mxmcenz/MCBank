using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Application.Interfaces;
using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Entities;
using MCBank.WebApi.Infrastructure;
using MCBank.WebApi.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MCBank.WebApi.Application;

public class AuthService(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<Result<TokenPairDto>> RegisterAsync(string username, string password)
    {
        if (await dbContext.Users.AnyAsync(u => u.Username == username))
        {
            return Result<TokenPairDto>.Failure("Пользователь с таким именем уже зарегистрирован");
        }

        var newUser = new User
        {
            Username = username,
            PasswordHash = passwordHasher.Hash(password)
        };

        await dbContext.Users.AddAsync(newUser);
        await dbContext.SaveChangesAsync();

        var accessToken = jwtService.GenerateAccessToken(newUser.Id);
        var refreshToken = jwtService.GenerateRefreshToken();

        return Result<TokenPairDto>.Success(new TokenPairDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresIn = _jwtSettings.AccessTokenExpirationMinutes,
            RefreshTokenExpiresIn = _jwtSettings.RefreshTokenExpirationDays * 24 * 60
        });
    }

    public async Task<Result<TokenPairDto>> LoginAsync(string username, string password)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || !passwordHasher.Verify(user.PasswordHash, password))
            return Result<TokenPairDto>.Failure("Неверное имя пользователя или пароль");

        var accessToken = jwtService.GenerateAccessToken(user.Id);
        var refreshToken = jwtService.GenerateRefreshToken();
        
        return Result<TokenPairDto>.Success(new TokenPairDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresIn = _jwtSettings.AccessTokenExpirationMinutes,
            RefreshTokenExpiresIn = _jwtSettings.RefreshTokenExpirationDays * 24 * 60
        });
    }
}