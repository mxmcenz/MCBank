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
        username = username.Trim();
        password = password.Trim();
        
        if (await dbContext.Users.AnyAsync(u => u.Username == username))
            return Result<TokenPairDto>.Failure("Пользователь с таким именем уже зарегистрирован", ErrorType.Conflict);

        var newUser = new User
        {
            Username = username,
            PasswordHash = passwordHasher.Hash(password)
        };

        await dbContext.Users.AddAsync(newUser);
        
        var accessToken = jwtService.GenerateAccessToken(newUser.Id);
        var refreshToken = jwtService.GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            IsRevoked = false,
            UserId = newUser.Id
        };

        await dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        
        await dbContext.SaveChangesAsync();

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
        username = username.Trim();
        password = password.Trim();
        
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || !passwordHasher.Verify(user.PasswordHash, password))
            return Result<TokenPairDto>.Failure("Неверное имя пользователя или пароль", ErrorType.Unauthorized);

        var accessToken = jwtService.GenerateAccessToken(user.Id);
        var refreshToken = jwtService.GenerateRefreshToken();
       
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            IsRevoked = false,
            UserId = user.Id
        };

        await dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await dbContext.SaveChangesAsync();

        return Result<TokenPairDto>.Success(new TokenPairDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresIn = _jwtSettings.AccessTokenExpirationMinutes,
            RefreshTokenExpiresIn = _jwtSettings.RefreshTokenExpirationDays * 24 * 60
        });
    }

    public async Task<Result<TokenPairDto>> RefreshAsync(string refreshToken)
    {
        var oldToken = await dbContext.RefreshTokens.Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (oldToken == null || oldToken.IsRevoked || oldToken.ExpiresAt < DateTime.UtcNow)
            return Result<TokenPairDto>.Failure("Сессия истекла", ErrorType.Unauthorized);

        dbContext.RefreshTokens.Remove(oldToken);

        var accessToken = jwtService.GenerateAccessToken(oldToken.UserId);
        var newRefreshToken = jwtService.GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            IsRevoked = false,
            UserId = oldToken.UserId
        };

        await dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await dbContext.SaveChangesAsync();
        
        return Result<TokenPairDto>.Success(new TokenPairDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresIn = _jwtSettings.AccessTokenExpirationMinutes,
            RefreshTokenExpiresIn = _jwtSettings.RefreshTokenExpirationDays * 24 * 60
        });
    }

    public async Task<Result> RevokeTokenAsync(string token)
    {
        var refreshToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);

        if (refreshToken == null)
        {
            return Result.Success();
        }
        
        refreshToken.IsRevoked = true;
        await dbContext.SaveChangesAsync();
        return Result.Success();
    }
}