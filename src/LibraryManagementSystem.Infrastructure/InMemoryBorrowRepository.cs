using LibraryManagementSystem.Domain;
using LibraryManagementSystem.Domain.Repositories;

namespace LibraryManagementSystem.Infrastructure;

public class InMemoryBorrowRepository : IBorrowRepository
{
    private readonly Dictionary<string, BorrowRecord> _borrowsDb = new();

    public void Add(BorrowRecord record)
    {
        if (record is null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (_borrowsDb.ContainsKey(record.BorrowId))
        {
            throw new InvalidOperationException($"Borrow record {record.BorrowId} already exists");
        }

        _borrowsDb[record.BorrowId] = record;
    }

    public BorrowRecord? GetById(string borrowId)
    {
        if (string.IsNullOrWhiteSpace(borrowId))
        {
            throw new ArgumentException("Borrow ID cannot be empty", nameof(borrowId));
        }

        return _borrowsDb.TryGetValue(borrowId, out var record) ? record : null;
    }

    public IReadOnlyCollection<BorrowRecord> GetAll()
    {
        return _borrowsDb.Values.ToList();
    }

    public void Delete(string borrowId)
    {
        if (string.IsNullOrWhiteSpace(borrowId))
        {
            throw new ArgumentException("Borrow ID cannot be empty", nameof(borrowId));
        }

        _borrowsDb.Remove(borrowId);
    }

    public List<BorrowRecord> GetByPatronId(string patronId)
    {
        if (string.IsNullOrWhiteSpace(patronId))
        {
            throw new ArgumentException("Patron ID cannot be empty", nameof(patronId));
        }

        return _borrowsDb.Values
            .Where(b => b.Patron.PatronId == patronId)
            .ToList();
    }

    public void Update(BorrowRecord record)
    {
        if (record is null)
        {
            throw new ArgumentNullException(nameof(record));
        }

        if (!_borrowsDb.ContainsKey(record.BorrowId))
        {
            throw new InvalidOperationException($"Borrow record {record.BorrowId} not found");
        }

        _borrowsDb[record.BorrowId] = record;
    }
}
