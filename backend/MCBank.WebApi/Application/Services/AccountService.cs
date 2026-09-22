using MCBank.WebApi.Application.DTOs;
using MCBank.WebApi.Application.Interfaces;
using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Core.Entities;
using MCBank.WebApi.Core.Enums;
using MCBank.WebApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MCBank.WebApi.Application.Services;

public class AccountService(AppDbContext dbContext, ISavingsPlanService savingsPlanService) : IAccountService
{
    public async Task<Result<AccountResponse>> GetAccountByIdAsync(int accountId, int currentUserId)
    {
        var account = await dbContext.Accounts.FindAsync(accountId);

        if (account == null)
            return Result<AccountResponse>.Failure("Счет не найден", ErrorType.NotFound);

        if (account.UserId != currentUserId)
            return Result<AccountResponse>.Failure("Доступ запрещен", ErrorType.Forbidden);

        var dto = new AccountResponse(
            account.Id,
            account.Iban,
            account.Balance,
            account.Type,
            account.ExpirationDate,
            account.InterestRate,
            account.CreatedAt);

        return Result<AccountResponse>.Success(dto);
    }

    public async Task<Result<List<AccountResponse>>> GetAllAccountsAsync(int userId)
    {
        var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .ToListAsync();

        var dto = accounts.Select(a => new AccountResponse(
            a.Id, a.Iban, a.Balance, a.Type, a.ExpirationDate, a.InterestRate, a.CreatedAt)).ToList();

        return Result<List<AccountResponse>>.Success(dto);
    }

    public async Task<Result<AccountResponse>> CreateAccountAsync(int userId, AccountType type, int? termMonths)
    {
        var userExists = await dbContext.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            return Result<AccountResponse>.Failure("Пользователь не найден", ErrorType.NotFound);

        var iban = $"KZ{string.Concat(Enumerable.Range(0, 18).Select(_ => Random.Shared.Next(0, 10)))}";
        decimal? interestRate = null;
        DateTime? expirationDate = null;

        if (type != AccountType.Current)
        {
            var plans = await savingsPlanService.GetSavingsPlansAsync();
            var plan = plans.Value.FirstOrDefault(p => p.Type == type && p.TermMonths == termMonths);
            if (plan == null)
                return Result<AccountResponse>.Failure("План не найден", ErrorType.NotFound);

            interestRate = plan.InterestRate;
            expirationDate = DateTime.UtcNow.AddMonths(plan.TermMonths);
        }

        var account = new Account
        {
            Iban = iban,
            UserId = userId,
            Balance = 0,
            Type = type,
            ExpirationDate = expirationDate,
            InterestRate = interestRate,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        var dto = new AccountResponse(
            account.Id,
            account.Iban,
            account.Balance,
            account.Type,
            account.ExpirationDate,
            account.InterestRate,
            account.CreatedAt);

        return Result<AccountResponse>.Success(dto);
    }

    public async Task<Result<int>> GetAccountIdByIbanAsync(string iban)
    {
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Iban == iban);
        return account == null
            ? Result<int>.Failure("Счет не найден", ErrorType.NotFound)
            : Result<int>.Success(account.Id);
    }

    public async Task<Result> DeleteAccount(int accountId, int currentUserId)
    {
        var account = await dbContext.Accounts.FindAsync(accountId);

        if (account == null)
            return Result.Failure("Счет не найден", ErrorType.NotFound);

        if (account.UserId != currentUserId)
            return Result.Failure("Доступ запрещен", ErrorType.Forbidden);

        account.IsDeleted = true;
        await dbContext.SaveChangesAsync();

        return Result.Success();
    }
}