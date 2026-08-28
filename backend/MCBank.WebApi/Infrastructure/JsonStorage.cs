using System.Text.Json;
using MCBank.WebApi.Core;
using MCBank.WebApi.Infrastructure.Interfaces;

namespace MCBank.WebApi.Infrastructure;

public class JsonStorage : IStorage
{
    private const string FilePath = "accounts.json";

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public async Task SaveAsync(List<Account> accounts)
    {
        try
        {
            var json = JsonSerializer.Serialize(accounts, _options);
            await File.WriteAllTextAsync(FilePath, json);
        }
        catch (Exception e)
        {
            throw new IOException("Не удалось сохранить данные в файл", e);
        }
    }

    public List<Account> Load()
    {
        if (!File.Exists(FilePath))
            return new List<Account>();

        try
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<Account>>(json, _options) ?? new List<Account>();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ошибка при загрузки данных: {e.Message}");
            return new List<Account>();
        }
    }
}