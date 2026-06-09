using LibraryManagementSystem.Domain;
using LibraryManagementSystem.Domain.Repositories;

namespace LibraryManagementSystem.Application;

public class LibraryQueryService
{
    private readonly IBookRepository _bookRepository;
    private readonly IBorrowRepository _borrowRepository;
    private readonly IOverduePolicy _overduePolicy;

    public LibraryQueryService(IBookRepository bookRepository, IBorrowRepository borrowRepository, IOverduePolicy? overduePolicy = null)
    {
        _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        _borrowRepository = borrowRepository ?? throw new ArgumentNullException(nameof(borrowRepository));
        _overduePolicy = overduePolicy ?? new OverduePolicy(feePerDay: 0.50m, gracePeriodDays: 0, maxBorrowDays: 30);
    }

    public List<Book> SearchBooks(string? title, string? author, string? category)
    {
        var query = _bookRepository.GetAll().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            query = query.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(b => b.Category.Contains(category, StringComparison.OrdinalIgnoreCase));
        }

        return query.OrderBy(b => b.Title).ToList();
    }

    public List<Book> SearchBooks(string query)
    {
        return SearchBooks(query, query, query);
    }

    public List<BorrowRecord> GetActiveBorrowRecords()
    {
        return _borrowRepository.GetAll()
            .Where(record => record.ReturnDate is null)
            .OrderBy(record => record.BorrowDate)
            .ToList();
    }

    public List<(Patron Patron, decimal OverdueDebt)> GetPatronsWithOverdueDebt()
    {
        return _borrowRepository.GetAll()
            .Where(record => record.ReturnDate is null && record.IsOverdue())
            .GroupBy(record => record.Patron)
            .Select(group => (Patron: group.Key, OverdueDebt: group.Sum(record => record.CalculateOverdueFee(_overduePolicy))))
            .OrderByDescending(item => item.OverdueDebt)
            .ThenBy(item => item.Patron.Name)
            .ToList();
    }

    public List<string> GetCategories()
    {
        return new HashSet<string>(_bookRepository.GetAll().Select(b => b.Category), StringComparer.OrdinalIgnoreCase)
            .OrderBy(category => category)
            .ToList();
    }

    public Dictionary<string, int> GetBorrowCountsByCategory()
    {
        return _borrowRepository.GetAll()
            .GroupBy(record => record.Copy.Book.Category)
            .ToDictionary(group => group.Key, group => group.Count());
    }

    public List<(Book Book, int BorrowCount)> GetTopBorrowedBooks(int top = 5)
    {
        return _borrowRepository.GetAll()
            .GroupBy(record => record.Copy.Book)
            .Select(group => (Book: group.Key, BorrowCount: group.Count()))
            .OrderByDescending(item => item.BorrowCount)
            .ThenBy(item => item.Book.Title)
            .Take(top)
            .ToList();
    }

    public List<BorrowRecord> GetOverdueBorrows()
    {
        return _borrowRepository.GetAll()
            .Where(record => record.ReturnDate is null && record.IsOverdue())
            .OrderBy(record => record.DueDate)
            .ToList();
    }

    public IEnumerable<Book> GetBooksByCategory(string category)
    {
        return _bookRepository.GetAll()
            .Where(book => string.Equals(book.Category, category, StringComparison.OrdinalIgnoreCase))
            .OrderBy(book => book.Title);
    }
}
