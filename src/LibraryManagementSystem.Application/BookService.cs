using LibraryManagementSystem.Domain;
using LibraryManagementSystem.Domain.Repositories;

namespace LibraryManagementSystem.Application;

public class BookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
    }

    public Book RegisterBook(string isbn, string title, string author, string category, int totalCopies)
    {
        var existingBook = _bookRepository.GetByIsbn(isbn);
        if (existingBook is not null)
        {
            throw new InvalidOperationException($"Book with ISBN {isbn} already exists");
        }

        var book = new Book(isbn, title, author, category, totalCopies);
        _bookRepository.Add(book);

        for (int i = 1; i <= totalCopies; i++)
        {
            var copy = new Copy($"{isbn}-{i}", book);
            _bookRepository.AddCopy(copy);
        }

        return book;
    }

    public Book? FindBook(string isbn)
    {
        return _bookRepository.GetByIsbn(isbn);
    }

    public List<Book> GetAllBooks()
    {
        return _bookRepository.GetAll().ToList();
    }

    public List<Book> GetAvailableBooks()
    {
        var allBooks = _bookRepository.GetAll();
        return allBooks.Where(book =>
        {
            var copies = _bookRepository.GetCopiesByIsbn(book.ISBN);
            return copies.Any(c => c.Status == CopyStatus.Available);
        }).ToList();
    }

    public int GetAvailableCopiesCount(string isbn)
    {
        var copies = _bookRepository.GetCopiesByIsbn(isbn);
        return copies.Count(c => c.Status == CopyStatus.Available);
    }

    public void UpdateBookCategory(string isbn, string category)
    {
        var book = _bookRepository.GetByIsbn(isbn);
        if (book is null)
        {
            throw new InvalidOperationException($"Book with ISBN {isbn} not found");
        }

        book.UpdateCategory(category);
    }

    public List<Copy> GetCopies(string isbn)
    {
        return _bookRepository.GetCopiesByIsbn(isbn);
    }

    public Copy? GetAvailableCopy(string isbn)
    {
        var copies = _bookRepository.GetCopiesByIsbn(isbn);
        return copies.FirstOrDefault(c => c.Status == CopyStatus.Available);
    }
}
