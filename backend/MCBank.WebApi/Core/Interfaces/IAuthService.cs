using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.DTOs;

namespace MCBank.WebApi.Core.Interfaces;

public interface IAuthService
{
    Task<Result<TokenPairDto>> RegisterAsync(string username, string password);
    Task<Result<TokenPairDto>> LoginAsync(string username, string password);
}