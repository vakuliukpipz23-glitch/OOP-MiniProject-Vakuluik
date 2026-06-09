using LibraryManagementSystem.Domain;

namespace LibraryManagementSystem.Infrastructure;

public interface ILibraryPersistenceService
{
    Task<LibrarySnapshot?> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(IEnumerable<Book> books, IEnumerable<Copy> copies, IEnumerable<Patron> patrons, IEnumerable<BorrowRecord> borrowRecords, CancellationToken cancellationToken = default);
    void RestoreState(LibrarySnapshot snapshot, LibraryManagementSystem.Domain.Repositories.IPatronRepository patronRepository, LibraryManagementSystem.Domain.Repositories.IBookRepository bookRepository, LibraryManagementSystem.Domain.Repositories.IBorrowRepository borrowRepository);
}
