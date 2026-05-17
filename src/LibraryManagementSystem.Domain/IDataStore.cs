namespace LibraryManagementSystem.Domain;

public interface IDataStore<T>
{
    Task<IReadOnlyCollection<T>> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(IReadOnlyCollection<T> items, CancellationToken cancellationToken = default);
}
