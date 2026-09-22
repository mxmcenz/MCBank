using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Enums;

namespace MCBank.WebApi.Application.Interfaces;

public interface IAccountService
{
    Task<Result<AccountResponse>> GetAccountByIdAsync(int accountId, int currentUserId);
    Task<Result<List<AccountResponse>>> GetAllAccountsAsync(int userId);
    Task<Result<AccountResponse>> CreateAccountAsync(int userId, AccountType type, int? termMonths);
    Task<Result<int>> GetAccountIdByIbanAsync(string iban);
    Task<Result> DeleteAccount(int accountId, int currentUserId);
}