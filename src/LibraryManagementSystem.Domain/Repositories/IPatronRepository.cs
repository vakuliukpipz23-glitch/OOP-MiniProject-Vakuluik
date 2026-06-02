namespace LibraryManagementSystem.Domain.Repositories;

public interface IPatronRepository : IRepository<Patron, string>
{
}

public interface ICopyRepository
{
    void AddCopy(Copy copy);
    List<Copy> GetCopiesByIsbn(string isbn);
    List<Copy> GetAllCopies();
    Copy? GetCopyById(string copyId);
    void UpdateCopy(Copy copy);
}

public interface IBookRepository : IRepository<Book, string>, ICopyRepository
{
    Book? GetByIsbn(string isbn);
}

public interface IBorrowRepository : IRepository<BorrowRecord, string>
{
    List<BorrowRecord> GetByPatronId(string patronId);
}
