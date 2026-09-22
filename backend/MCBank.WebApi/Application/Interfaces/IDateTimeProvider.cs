namespace MCBank.WebApi.Application.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}