using System.Text.Json;
using LibraryManagementSystem.Domain;

namespace LibraryManagementSystem.Infrastructure;

public class JsonFileDataStore<T> : IDataStore<T>
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public JsonFileDataStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty", nameof(filePath));
        }

        _filePath = filePath;
        var folder = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
    }

    public async Task<IReadOnlyCollection<T>> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return Array.Empty<T>();
        }

        try
        {
            using var stream = File.OpenRead(_filePath);
            var items = await JsonSerializer.DeserializeAsync<List<T>>(stream, _options, cancellationToken);
            return items ?? new List<T>();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Data file is corrupt or invalid JSON: {_filePath}", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Cannot read data file: {_filePath}", ex);
        }
    }

    public async Task SaveAsync(IReadOnlyCollection<T> items, CancellationToken cancellationToken = default)
    {
        try
        {
            using var stream = File.Open(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(stream, items, _options, cancellationToken);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Cannot write data file: {_filePath}", ex);
        }
    }
}
