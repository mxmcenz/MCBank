using MCBank.WebApi.Application.Interfaces;
using MCBank.WebApi.Application.Strategies;
using MCBank.WebApi.Core.Entities;
using MCBank.WebApi.Core.Enums;
using MCBank.WebApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MCBank.WebApi.Application.Services;

public class InterestService(
    AppDbContext dbContext,
    IAccountStrategyFactory accountFactory,
    IDateTimeProvider dateTimeProvider,
    ILogger<InterestService> logger) : IInterestService
{
    public async Task ApplyInterestAsync()
    {
        var now = dateTimeProvider.UtcNow;
        
        var accountsQuery =
            dbContext.Accounts.Where(a => a.Type != AccountType.Current && !a.IsDeleted && a.InterestRate != null);

        var accountsToProcess = await accountsQuery.ToListAsync();

        foreach (var account in accountsToProcess)
        {
            var strategy = accountFactory.GetStrategy(account.Type);

            if (!strategy.IsInterestDue(account.LastInterestAppliedAt, now))
                continue;

            if (!account.InterestRate.HasValue)
            {
                logger.LogWarning("Счет {AccountId} имеет тип, требующий начисления, но ставка не задана.",
                    account.Id);
                continue;
            }

            var interest = strategy.CalculateInterest(account.Balance, account.InterestRate.Value);

            if (interest <= 0)
                continue;

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            
            try
            {
                var interestTransaction = new Transaction
                {
                    Amount = interest,
                    Type = TransactionType.Interest,
                    CreatedAt = DateTime.UtcNow,
                    AccountId = account.Id,
                    IsDeleted = false
                };

                account.Balance += interest;
                account.LastInterestAppliedAt = now;

                await dbContext.Transactions.AddAsync(interestTransaction);
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                
                dbContext.ChangeTracker.Clear();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при начислении процентов для счета {AccountId}", account.Id);
                await transaction.RollbackAsync();
            }
        }
    }
}