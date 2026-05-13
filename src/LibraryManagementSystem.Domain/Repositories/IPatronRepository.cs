namespace LibraryManagementSystem.Domain.Repositories;

public interface IPatronRepository
{
    void Add(Patron patron);
    Patron? GetById(string patronId);
    List<Patron> GetAll();
    void Update(Patron patron);
}

public interface IBookRepository
{
    void Add(Book book);
    Book? GetByIsbn(string isbn);
    List<Book> GetAll();
    void Update(Book book);
    void AddCopy(Copy copy);
    List<Copy> GetCopiesByIsbn(string isbn);
    Copy? GetCopyById(string copyId);
    void UpdateCopy(Copy copy);
}

public interface IBorrowRepository
{
    void Add(BorrowRecord record);
    BorrowRecord? GetById(string borrowId);
    List<BorrowRecord> GetAll();
    List<BorrowRecord> GetByPatronId(string patronId);
    void Update(BorrowRecord record);
}
