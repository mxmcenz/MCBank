using MCBank.WebApi.Core.Common;
using MCBank.WebApi.Infrastructure.Settings;

namespace MCBank.WebApi.Application.Interfaces;

public interface ISavingsPlanService
{
    Task<Result<List<SavingsPlan>>> GetSavingsPlansAsync();
}