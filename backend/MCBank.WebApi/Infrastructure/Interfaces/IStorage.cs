using MCBank.WebApi.Core;

namespace MCBank.WebApi.Infrastructure.Interfaces;

public interface IStorage
{
    Task SaveAsync(List<Account> accounts);
    List<Account> Load();
}