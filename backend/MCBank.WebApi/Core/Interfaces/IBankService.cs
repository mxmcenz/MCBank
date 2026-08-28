using MCBank.WebApi.Core.Common;

namespace MCBank.WebApi.Core.Interfaces;

public interface IBankService
{
    Task<Result<Account>> GetAccountByIdAsync(int id);
    Task<Result<List<Account>>> GetAllAccountsAsync();
    Task<Result<Account>> CreateAccountAsync();
    Task<Result> DepositAsync(int accountId, decimal amount);
    Task<Result> WithdrawAsync(int accountId, decimal amount);
    Task<Result> TransferAsync(int fromAccountId, int toAccountId, decimal amount);
}