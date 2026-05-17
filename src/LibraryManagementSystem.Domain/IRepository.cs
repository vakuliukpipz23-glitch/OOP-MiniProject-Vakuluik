namespace LibraryManagementSystem.Domain;

public interface IRepository<T, TId>
{
    IReadOnlyCollection<T> GetAll();
    T? GetById(TId id);
    void Add(T entity);
    void Update(T entity);
    void Delete(TId id);
}
