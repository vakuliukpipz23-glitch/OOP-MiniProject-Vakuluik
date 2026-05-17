using LibraryManagementSystem.Domain;
using LibraryManagementSystem.Domain.Repositories;

namespace LibraryManagementSystem.Application;

public class BorrowService
{
    private readonly IBorrowRepository _borrowRepository;
    private readonly IPatronRepository _patronRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IOverduePolicy _overduePolicy;
    private const decimal MaxAllowedDebt = 0m;

    public BorrowService(
        IBorrowRepository borrowRepository,
        IPatronRepository patronRepository,
        IBookRepository bookRepository,
        IOverduePolicy? overduePolicy = null)
    {
        _borrowRepository = borrowRepository ?? throw new ArgumentNullException(nameof(borrowRepository));
        _patronRepository = patronRepository ?? throw new ArgumentNullException(nameof(patronRepository));
        _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        _overduePolicy = overduePolicy ?? new OverduePolicy(feePerDay: 0.50m, gracePeriodDays: 0, maxBorrowDays: 30);
    }

    public BorrowRecord BorrowBook(string patronId, string isbn)
    {
        var patron = _patronRepository.GetById(patronId);
        if (patron is null)
        {
            throw new InvalidOperationException($"Patron {patronId} not found");
        }

        var outstandingDebt = GetPatronTotalDebts(patronId);
        if (outstandingDebt > MaxAllowedDebt)
        {
            throw new InvalidOperationException(
                $"Patron {patronId} has overdue fees of {outstandingDebt:C} and cannot borrow new books until the debt is cleared.");
        }

        var book = _bookRepository.GetByIsbn(isbn);
        if (book is null)
        {
            throw new InvalidOperationException($"Book with ISBN {isbn} not found");
        }

        var availableCopy = GetAvailableCopy(isbn);
        if (availableCopy is null)
        {
            throw new InvalidOperationException(
                $"No available copies of '{book.Title}' (ISBN: {isbn}). " +
                $"Total copies: {book.TotalCopies}");
        }

        string borrowId = GenerateBorrowId();
        var borrowRecord = new BorrowRecord(borrowId, patron, availableCopy, _overduePolicy);

        _borrowRepository.Add(borrowRecord);
        _bookRepository.UpdateCopy(availableCopy);

        return borrowRecord;
    }

    public BorrowRecord ReturnBook(string borrowId)
    {
        var borrowRecord = _borrowRepository.GetById(borrowId);
        if (borrowRecord is null)
        {
            throw new InvalidOperationException($"Borrow record {borrowId} not found");
        }

        if (borrowRecord.ReturnDate.HasValue)
        {
            throw new InvalidOperationException("This book has already been returned");
        }

        borrowRecord.ReturnBook();
        borrowRecord.CalculateOverdueFee(_overduePolicy);

        _borrowRepository.Update(borrowRecord);
        _bookRepository.UpdateCopy(borrowRecord.Copy);

        return borrowRecord;
    }

    public BorrowRecord? GetBorrowRecord(string borrowId)
    {
        return _borrowRepository.GetById(borrowId);
    }

    public List<BorrowRecord> GetPatronBorrows(string patronId)
    {
        return _borrowRepository.GetByPatronId(patronId);
    }

    public List<BorrowRecord> GetPatronActiveBorrows(string patronId)
    {
        return _borrowRepository.GetByPatronId(patronId)
            .Where(b => !b.ReturnDate.HasValue)
            .ToList();
    }

    public List<BorrowRecord> GetPatronOverdueBorrows(string patronId)
    {
        return _borrowRepository.GetByPatronId(patronId)
            .Where(b => !b.ReturnDate.HasValue && b.IsOverdue())
            .ToList();
    }

    public decimal GetPatronTotalDebts(string patronId)
    {
        return _borrowRepository.GetByPatronId(patronId)
            .Where(b => !b.ReturnDate.HasValue)
            .Sum(b => b.CalculateOverdueFee(_overduePolicy));
    }

    public List<BorrowRecord> GetAllBorrows()
    {
        return _borrowRepository.GetAll().ToList();
    }

    private Copy? GetAvailableCopy(string isbn)
    {
        return _bookRepository.GetCopiesByIsbn(isbn)
            .FirstOrDefault(c => c.Status == CopyStatus.Available);
    }

    private static string GenerateBorrowId()
    {
        return $"B{DateTime.UtcNow.Ticks}";
    }
}
