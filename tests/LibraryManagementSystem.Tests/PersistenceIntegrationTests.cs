using LibraryManagementSystem.Domain;
using LibraryManagementSystem.Infrastructure;
using LibraryManagementSystem.Domain.Repositories;
using Xunit;

namespace LibraryManagementSystem.Tests;

public class PersistenceIntegrationTests : IDisposable
{
    private readonly string _tempFilePath;

    public PersistenceIntegrationTests()
    {
        _tempFilePath = Path.Combine(Path.GetTempPath(), $"library-persistence-test-{Guid.NewGuid():N}.json");
    }

    [Fact]
    public async Task JsonFileDataStore_SaveAsync_PersistsItemsToDisk()
    {
        // Arrange
        var store = new JsonFileDataStore<Book>(_tempFilePath);
        var books = new[] { new Book("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 2) };

        // Act
        await store.SaveAsync(books);
        var loadedBooks = await store.LoadAsync();

        // Assert
        Assert.Single(loadedBooks);
        Assert.Equal("978-0134685991", loadedBooks.Single().ISBN);
    }

    [Fact]
    public async Task JsonFileDataStore_LoadAsync_ReturnsEmptyCollectionWhenFileMissing()
    {
        // Arrange
        var store = new JsonFileDataStore<Book>(_tempFilePath);

        // Act
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }

        var loadedBooks = await store.LoadAsync();

        // Assert
        Assert.Empty(loadedBooks);
    }

    [Fact]
    public async Task JsonFileDataStore_LoadAsync_ThrowsDataCorruptionExceptionForInvalidJson()
    {
        // Arrange
        File.WriteAllText(_tempFilePath, "{ invalid-json }");
        var store = new JsonFileDataStore<Book>(_tempFilePath);

        // Act & Assert
        await Assert.ThrowsAsync<DataCorruptionException>(() => store.LoadAsync());
    }

    [Fact]
    public async Task LibraryPersistenceService_SaveAsync_AndLoadAsync_RoundTripsLibraryState()
    {
        // Arrange
        var dataStore = new JsonFileDataStore<LibrarySnapshot>(_tempFilePath);
        var persistenceService = new LibraryPersistenceService(dataStore);

        var book = new Book("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 1);
        var copy = new Copy("COPY-001", book);
        var patron = new Patron("P001", "John Doe", "john@example.com", "+1234567890");
        var policy = new OverduePolicy();
        var borrowRecord = new BorrowRecord("B001", patron, copy, policy);

        await persistenceService.SaveAsync(new[] { book }, new[] { copy }, new[] { patron }, new[] { borrowRecord });

        // Act
        var loadedSnapshot = await persistenceService.LoadAsync();

        // Assert
        Assert.NotNull(loadedSnapshot);
        Assert.Single(loadedSnapshot.Books);
        Assert.Single(loadedSnapshot.Copies);
        Assert.Single(loadedSnapshot.Patrons);
        Assert.Single(loadedSnapshot.BorrowRecords);
    }

    [Fact]
    public void LibraryPersistenceService_RestoreState_RebuildsDomainStateFromSnapshot()
    {
        // Arrange
        var bookRepo = new InMemoryBookRepository();
        var patronRepo = new InMemoryPatronRepository();
        var borrowRepo = new InMemoryBorrowRepository();
        var persistenceService = new LibraryPersistenceService(new JsonFileDataStore<LibrarySnapshot>(_tempFilePath));

        var snapshot = new LibrarySnapshot(
            new List<BookDto> { new("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 1) },
            new List<CopyDto> { new("COPY-001", "978-0134685991", CopyStatus.Available) },
            new List<PatronDto> { new("P001", "John Doe", "john@example.com", "+1234567890", DateTime.UtcNow) },
            new List<BorrowRecordDto> { new("B001", "P001", "COPY-001", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), null, 0m) }
        );

        // Act
        persistenceService.RestoreState(snapshot, patronRepo, bookRepo, borrowRepo);

        // Assert
        Assert.NotNull(bookRepo.GetByIsbn("978-0134685991"));
        Assert.NotNull(patronRepo.GetById("P001"));
        Assert.NotNull(borrowRepo.GetById("B001"));
    }

    [Fact]
    public void LibraryPersistenceService_RestoreState_ThrowsPersistenceRestoreException_WhenCopyBookMissing()
    {
        // Arrange
        var bookRepo = new InMemoryBookRepository();
        var patronRepo = new InMemoryPatronRepository();
        var borrowRepo = new InMemoryBorrowRepository();
        var persistenceService = new LibraryPersistenceService(new JsonFileDataStore<LibrarySnapshot>(_tempFilePath));

        var snapshot = new LibrarySnapshot(
            new List<BookDto>(),
            new List<CopyDto> { new("COPY-001", "978-0134685991", CopyStatus.Available) },
            new List<PatronDto> { new("P001", "John Doe", "john@example.com", "+1234567890", DateTime.UtcNow) },
            new List<BorrowRecordDto>()
        );

        // Act & Assert
        Assert.Throws<PersistenceRestoreException>(() => persistenceService.RestoreState(snapshot, patronRepo, bookRepo, borrowRepo));
    }

    [Fact]
    public void LibraryPersistenceService_RestoreState_ThrowsPersistenceRestoreException_WhenBorrowCopyMissing()
    {
        // Arrange
        var bookRepo = new InMemoryBookRepository();
        var patronRepo = new InMemoryPatronRepository();
        var borrowRepo = new InMemoryBorrowRepository();
        var persistenceService = new LibraryPersistenceService(new JsonFileDataStore<LibrarySnapshot>(_tempFilePath));

        var snapshot = new LibrarySnapshot(
            new List<BookDto> { new("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 1) },
            new List<CopyDto> { new("COPY-001", "978-0134685991", CopyStatus.Available) },
            new List<PatronDto> { new("P001", "John Doe", "john@example.com", "+1234567890", DateTime.UtcNow) },
            new List<BorrowRecordDto> { new("B001", "P001", "MISSING-COPY", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), null, 0m) }
        );

        // Act & Assert
        Assert.Throws<PersistenceRestoreException>(() => persistenceService.RestoreState(snapshot, patronRepo, bookRepo, borrowRepo));
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }
}
