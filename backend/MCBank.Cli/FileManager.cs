using System.Text.Json;

namespace MCBank.Cli;

public class FileManager
{
    private const string FilePath = "accounts.json";

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public void Save(List<BankAccount> accounts)
    {
        var json = JsonSerializer.Serialize(accounts, _options);
        File.WriteAllText(FilePath, json);
    }

    public List<BankAccount> Load()
    {
        if (!File.Exists(FilePath))
            return new List<BankAccount>();

        try
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<BankAccount>>(json) ?? new List<BankAccount>();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ошибка при загрузки данных: {e.Message}");
            return new List<BankAccount>();
        }
    }
}