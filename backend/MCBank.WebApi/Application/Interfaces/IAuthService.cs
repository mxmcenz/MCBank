using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Core.Common;

namespace MCBank.WebApi.Application.Interfaces;

public interface IAuthService
{
    Task<Result<TokenPairDto>> RegisterAsync(string username, string password);
    Task<Result<TokenPairDto>> LoginAsync(string username, string password);
}