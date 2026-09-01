using MCBank.WebApi.Application.Interfaces;

namespace MCBank.WebApi.Infrastructure.Authentication;

public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string hash, string password) => BCrypt.Net.BCrypt.Verify(password, hash);
}