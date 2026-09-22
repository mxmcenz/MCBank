using MCBank.WebApi.Application.Interfaces;

namespace MCBank.WebApi.Application.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}