namespace MCBank.WebApi.Core.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(int userId);
    string GenerateRefreshToken();
}