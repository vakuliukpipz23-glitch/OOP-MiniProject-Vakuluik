using System.Text.Json;
using LibraryManagementSystem.Domain;

namespace LibraryManagementSystem.Infrastructure;

public class JsonFileDataStore<T> : IDataStore<T>
{
    private const int MaxRetryAttempts = 3;
    private const int RetryDelayMilliseconds = 100;

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

        return await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                using var stream = File.OpenRead(_filePath);
                var items = await JsonSerializer.DeserializeAsync<List<T>>(stream, _options, cancellationToken);
                return items ?? new List<T>();
            }
            catch (JsonException ex)
            {
                throw new DataCorruptionException($"Data file is corrupt or invalid JSON: {_filePath}", ex);
            }
        }, cancellationToken, failureTransform: ex => new DataLoadException($"Cannot read data file: {_filePath}", ex));
    }

    public async Task SaveAsync(IReadOnlyCollection<T> items, CancellationToken cancellationToken = default)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        await ExecuteWithRetryAsync(async () =>
        {
            using var stream = File.Open(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(stream, items, _options, cancellationToken);
        }, cancellationToken, failureTransform: ex => new DataSaveException($"Cannot write data file: {_filePath}", ex));
    }

    private static async Task<TResult> ExecuteWithRetryAsync<TResult>(Func<Task<TResult>> work, CancellationToken cancellationToken, Func<IOException, Exception> failureTransform)
    {
        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                return await work();
            }
            catch (IOException) when (attempt < MaxRetryAttempts)
            {
                await Task.Delay(RetryDelayMilliseconds, cancellationToken);
            }
            catch (IOException ex)
            {
                throw failureTransform(ex);
            }
        }

        throw new DataLoadException("Unable to complete file operation after multiple retry attempts.");
    }

    private static async Task ExecuteWithRetryAsync(Func<Task> work, CancellationToken cancellationToken, Func<IOException, Exception> failureTransform)
    {
        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                await work();
                return;
            }
            catch (IOException) when (attempt < MaxRetryAttempts)
            {
                await Task.Delay(RetryDelayMilliseconds, cancellationToken);
            }
            catch (IOException ex)
            {
                throw failureTransform(ex);
            }
        }

        throw new DataSaveException("Unable to complete file operation after multiple retry attempts.");
    }
}
