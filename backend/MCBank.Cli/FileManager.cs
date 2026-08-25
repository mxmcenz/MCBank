using System.Text.Json;

namespace MCBank.Cli;

public class FileManager
{
    private const string FilePath = "wallet.json";

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public void Save(List<Transaction> transactions)
    {
        var json = JsonSerializer.Serialize(transactions, _options);
        File.WriteAllText(FilePath, json);
    }

    public List<Transaction> Load()
    {
        if (!File.Exists(FilePath))
            return new List<Transaction>();

        try
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ошибка при загрузки данных: {e.Message}");
            return new List<Transaction>();
        }
    }
}