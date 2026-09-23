using MCBank.WebApi.Application.Interfaces;
using Quartz;

namespace MCBank.WebApi.Application.Jobs;

public class InterestCalculationJob(IInterestService interestService, ILogger<InterestCalculationJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Запуск автоматического начисления процентов...");

        try
        {
            await interestService.ApplyInterestAsync();
            logger.LogInformation("Начисление процентов успешно завершено.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Критическая ошибка при выполнении фоновой задачи начисления процентов.");
        }
    }
}