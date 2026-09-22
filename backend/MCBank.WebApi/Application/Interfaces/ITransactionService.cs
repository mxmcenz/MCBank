using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Entities;

namespace MCBank.WebApi.Application.Interfaces;

public interface ITransactionService
{
    Task<Result> DepositAsync(int accountId, int currentUserId, decimal amount);
    Task<Result> WithdrawAsync(int accountId, int currentUserId, decimal amount);
    Task<Result> TransferAsync(int fromAccountId, int toAccountId, int currentUserId, decimal amount);
    Task<Result<List<Transaction>>> GetTransactionHistoryAsync(int accountId, int currentUserId);
}