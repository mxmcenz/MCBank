using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Entities;

namespace MCBank.WebApi.Application.Interfaces;

public interface IBankService
{
    Task<Result<AccountResponse>> GetAccountByIdAsync(int accountId, int currentUserId);
    Task<Result<List<AccountResponse>>> GetAllAccountsAsync(int userId);
    Task<Result<AccountResponse>> CreateAccountAsync(int userId);
    Task<Result> DepositAsync(int accountId, int currentUserId, decimal amount);
    Task<Result> WithdrawAsync(int accountId, int currentUserId, decimal amount);
    Task<Result> TransferAsync(int fromAccountId, int toAccountId, int currentUserId, decimal amount);
    Task<Result<List<Transaction>>> GetTransactionHistoryAsync(int accountId, int currentUserId);
    Task<Result> DeleteAccount(int accountId, int currentUserId);
}