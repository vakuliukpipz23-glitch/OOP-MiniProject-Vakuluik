using LibraryManagementSystem.Domain;
using LibraryManagementSystem.Domain.Repositories;

namespace LibraryManagementSystem.Infrastructure;

public class LibraryPersistenceService : ILibraryPersistenceService
{
    private readonly IDataStore<LibrarySnapshot> _dataStore;

    public LibraryPersistenceService(IDataStore<LibrarySnapshot> dataStore)
    {
        _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
    }

    public async Task<LibrarySnapshot?> LoadAsync(CancellationToken cancellationToken = default)
    {
        var items = await _dataStore.LoadAsync(cancellationToken);
        return items.FirstOrDefault();
    }

    public async Task SaveAsync(IEnumerable<Book> books, IEnumerable<Copy> copies, IEnumerable<Patron> patrons, IEnumerable<BorrowRecord> borrowRecords, CancellationToken cancellationToken = default)
    {
        var snapshot = new LibrarySnapshot(
            books.Select(b => new BookDto(b.ISBN, b.Title, b.Author, b.Category, b.TotalCopies)).ToList(),
            copies.Select(c => new CopyDto(c.CopyId, c.Book.ISBN, c.Status)).ToList(),
            patrons.Select(p => new PatronDto(p.PatronId, p.Name, p.Email, p.Phone, p.RegistrationDate)).ToList(),
            borrowRecords.Select(r => new BorrowRecordDto(r.BorrowId, r.Patron.PatronId, r.Copy.CopyId, r.BorrowDate, r.DueDate, r.ReturnDate, r.OverdueFee)).ToList()
        );

        await _dataStore.SaveAsync(new[] { snapshot }, cancellationToken);
    }

    public void RestoreState(LibrarySnapshot snapshot, IPatronRepository patronRepository, IBookRepository bookRepository, IBorrowRepository borrowRepository)
    {
        if (snapshot is null)
        {
            return;
        }

        var books = snapshot.Books.ToDictionary(b => b.ISBN, b => new Book(b.ISBN, b.Title, b.Author, b.Category, b.TotalCopies));

        foreach (var book in books.Values)
        {
            bookRepository.Add(book);
        }

        var copies = new Dictionary<string, Copy>();
        foreach (var copyDto in snapshot.Copies)
        {
            if (!books.TryGetValue(copyDto.BookIsbn, out var book))
            {
                throw new InvalidOperationException($"Cannot restore copy {copyDto.CopyId}: book {copyDto.BookIsbn} missing.");
            }

            var copy = new Copy(copyDto.CopyId, book);
            SetCopyStatus(copy, copyDto.Status);
            copies[copy.CopyId] = copy;
            bookRepository.AddCopy(copy);
        }

        var patrons = new Dictionary<string, Patron>();
        foreach (var patronDto in snapshot.Patrons)
        {
            var patron = new Patron(patronDto.PatronId, patronDto.Name, patronDto.Email, patronDto.Phone, patronDto.RegistrationDate);
            patronRepository.Add(patron);
            patrons[patron.PatronId] = patron;
        }

        foreach (var borrowDto in snapshot.BorrowRecords)
        {
            if (!patrons.TryGetValue(borrowDto.PatronId, out var patron))
            {
                throw new InvalidOperationException($"Cannot restore borrow record {borrowDto.BorrowId}: patron {borrowDto.PatronId} missing.");
            }

            if (!copies.TryGetValue(borrowDto.CopyId, out var copy))
            {
                throw new InvalidOperationException($"Cannot restore borrow record {borrowDto.BorrowId}: copy {borrowDto.CopyId} missing.");
            }

            var borrowRecord = BorrowRecord.Rehydrate(borrowDto.BorrowId, patron, copy, borrowDto.BorrowDate, borrowDto.DueDate, borrowDto.ReturnDate, borrowDto.OverdueFee);
            borrowRepository.Add(borrowRecord);
        }
    }

    private static void SetCopyStatus(Copy copy, CopyStatus status)
    {
        if (status == CopyStatus.Available)
        {
            return;
        }

        if (status == CopyStatus.Borrowed)
        {
            copy.MarkAsBorrowed();
            return;
        }

        if (status == CopyStatus.Reserved)
        {
            copy.MarkAsReserved();
            return;
        }

        if (status == CopyStatus.Damaged)
        {
            copy.MarkAsDamaged();
            return;
        }
    }
}
