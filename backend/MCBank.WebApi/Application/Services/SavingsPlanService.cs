using MCBank.WebApi.Application.Interfaces;
using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace MCBank.WebApi.Application.Services;

public class SavingsPlanService(IOptions<SavingsSettings> savingsOptions) : ISavingsPlanService
{
    private readonly SavingsSettings _savingsSettings = savingsOptions.Value;

    public Task<Result<List<SavingsPlan>>> GetSavingsPlansAsync() =>
        Task.FromResult(Result<List<SavingsPlan>>.Success(_savingsSettings.Plans));
}