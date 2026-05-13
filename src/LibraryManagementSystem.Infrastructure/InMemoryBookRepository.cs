using LibraryManagementSystem.Domain;
using LibraryManagementSystem.Domain.Repositories;

namespace LibraryManagementSystem.Infrastructure;

public class InMemoryBookRepository : IBookRepository
{
    private readonly Dictionary<string, Book> _booksDb = new();
    private readonly Dictionary<string, Copy> _copiesDb = new();
    private readonly List<string> _bookIsbnIndex = new();

    public void Add(Book book)
    {
        if (book is null)
        {
            throw new ArgumentNullException(nameof(book));
        }

        if (_booksDb.ContainsKey(book.ISBN))
        {
            throw new InvalidOperationException($"Book with ISBN {book.ISBN} already exists");
        }

        _booksDb[book.ISBN] = book;
        _bookIsbnIndex.Add(book.ISBN);
    }

    public Book? GetByIsbn(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentException("ISBN cannot be empty", nameof(isbn));
        }

        return _booksDb.TryGetValue(isbn, out var book) ? book : null;
    }

    public List<Book> GetAll()
    {
        return _booksDb.Values.ToList();
    }

    public void Update(Book book)
    {
        if (book is null)
        {
            throw new ArgumentNullException(nameof(book));
        }

        if (!_booksDb.ContainsKey(book.ISBN))
        {
            throw new InvalidOperationException($"Book with ISBN {book.ISBN} not found");
        }

        _booksDb[book.ISBN] = book;
    }

    public void AddCopy(Copy copy)
    {
        if (copy is null)
        {
            throw new ArgumentNullException(nameof(copy));
        }

        if (_copiesDb.ContainsKey(copy.CopyId))
        {
            throw new InvalidOperationException($"Copy {copy.CopyId} already exists");
        }

        _copiesDb[copy.CopyId] = copy;
    }

    public List<Copy> GetCopiesByIsbn(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentException("ISBN cannot be empty", nameof(isbn));
        }

        return _copiesDb.Values
            .Where(c => c.Book.ISBN == isbn)
            .ToList();
    }

    public Copy? GetCopyById(string copyId)
    {
        if (string.IsNullOrWhiteSpace(copyId))
        {
            throw new ArgumentException("Copy ID cannot be empty", nameof(copyId));
        }

        return _copiesDb.TryGetValue(copyId, out var copy) ? copy : null;
    }

    public void UpdateCopy(Copy copy)
    {
        if (copy is null)
        {
            throw new ArgumentNullException(nameof(copy));
        }

        if (!_copiesDb.ContainsKey(copy.CopyId))
        {
            throw new InvalidOperationException($"Copy {copy.CopyId} not found");
        }

        _copiesDb[copy.CopyId] = copy;
    }
}
