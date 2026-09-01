namespace MCBank.WebApi.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(int userId);
    string GenerateRefreshToken();
}