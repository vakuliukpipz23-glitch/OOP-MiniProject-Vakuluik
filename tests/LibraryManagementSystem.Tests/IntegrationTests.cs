using LibraryManagementSystem.Domain;
using LibraryManagementSystem.Infrastructure;
using LibraryManagementSystem.Application;
using System.IO;
using Xunit;

namespace LibraryManagementSystem.Tests;

public class IntegrationTests
{
    [Fact]
    public async Task SaveAndReload_PreservesAggregateState()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var dataStore = new JsonFileDataStore<LibrarySnapshot>(tempFile);
            var persistence = new LibraryPersistenceService(dataStore);

            var bookRepo = new InMemoryBookRepository();
            var patronRepo = new InMemoryPatronRepository();
            var borrowRepo = new InMemoryBorrowRepository();

            var book = new Book("978-0134685991", "T1", "A", "Cat", 1);
            var copy = new Copy("C1", book);
            var patron = new Patron("P1", "Name", "e@e.com", "+1");
            var policy = new OverduePolicy();

            bookRepo.Add(book);
            bookRepo.AddCopy(copy);
            patronRepo.Add(patron);

            var borrow = BorrowRecord.Rehydrate("B1", patron, copy, DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, 0);
            borrowRepo.Add(borrow);

            await persistence.SaveAsync(bookRepo.GetAll(), bookRepo.GetAllCopies(), patronRepo.GetAll(), borrowRepo.GetAll());

            var reloadedDataStore = new JsonFileDataStore<LibrarySnapshot>(tempFile);
            var reloadedPersistence = new LibraryPersistenceService(reloadedDataStore);

            var bookRepo2 = new InMemoryBookRepository();
            var patronRepo2 = new InMemoryPatronRepository();
            var borrowRepo2 = new InMemoryBorrowRepository();

            var snapshot = await reloadedPersistence.LoadAsync();
            Assert.NotNull(snapshot);

            reloadedPersistence.RestoreState(snapshot!, patronRepo2, bookRepo2, borrowRepo2);

            Assert.Single(bookRepo2.GetAll());
            Assert.Single(patronRepo2.GetAll());
            Assert.Single(borrowRepo2.GetAll());
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveAndReload_AllowsBusinessOperationsAfterRestore()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var dataStore = new JsonFileDataStore<LibrarySnapshot>(tempFile);
            var persistence = new LibraryPersistenceService(dataStore);

            var bookRepo = new InMemoryBookRepository();
            var patronRepo = new InMemoryPatronRepository();
            var borrowRepo = new InMemoryBorrowRepository();

            var book = new Book("978-0201633610", "T2", "A", "Cat", 2);
            var copy1 = new Copy("C2-1", book);
            var copy2 = new Copy("C2-2", book);
            var patron = new Patron("P2", "Name2", "e2@e.com", "+2");

            bookRepo.Add(book);
            bookRepo.AddCopy(copy1);
            bookRepo.AddCopy(copy2);
            patronRepo.Add(patron);

            await persistence.SaveAsync(bookRepo.GetAll(), bookRepo.GetAllCopies(), patronRepo.GetAll(), borrowRepo.GetAll());

            // Reload
            var reloadedPersistence = new LibraryPersistenceService(new JsonFileDataStore<LibrarySnapshot>(tempFile));
            var bookRepo2 = new InMemoryBookRepository();
            var patronRepo2 = new InMemoryPatronRepository();
            var borrowRepo2 = new InMemoryBorrowRepository();

            var snapshot = await reloadedPersistence.LoadAsync();
            reloadedPersistence.RestoreState(snapshot!, patronRepo2, bookRepo2, borrowRepo2);

            // Perform business operation after restore
            var service = new BorrowService(borrowRepo2, patronRepo2, bookRepo2);
            var borrow = service.BorrowBook("P2", "978-0201633610");

            Assert.NotNull(borrow);
            Assert.Equal("P2", borrow.Patron.PatronId);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task Load_NonexistentFile_ReturnsNullSnapshot()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        var dataStore = new JsonFileDataStore<LibrarySnapshot>(tempFile);
        var persistence = new LibraryPersistenceService(dataStore);

        var snapshot = await persistence.LoadAsync();
        Assert.Null(snapshot);
    }

    [Fact]
    public async Task Load_CorruptedFile_ThrowsInvalidOperationException()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            // Create corrupted file content
            await File.WriteAllTextAsync(tempFile, "{ this is not valid json }");

            var dataStore = new JsonFileDataStore<LibrarySnapshot>(tempFile);
            var persistence = new LibraryPersistenceService(dataStore);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await persistence.LoadAsync());
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task Save_WhenFileLocked_ThrowsInvalidOperationException()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            // Create file and lock it
            using var fs = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            var dataStore = new JsonFileDataStore<LibrarySnapshot>(tempFile);
            var persistence = new LibraryPersistenceService(dataStore);

            var bookRepo = new InMemoryBookRepository();
            var patronRepo = new InMemoryPatronRepository();
            var borrowRepo = new InMemoryBorrowRepository();

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await persistence.SaveAsync(bookRepo.GetAll(), bookRepo.GetAllCopies(), patronRepo.GetAll(), borrowRepo.GetAll()));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task MultipleSequentialSaves_NoDataLoss()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            var dataStore = new JsonFileDataStore<LibrarySnapshot>(tempFile);
            var persistence = new LibraryPersistenceService(dataStore);

            var bookRepo = new InMemoryBookRepository();
            var patronRepo = new InMemoryPatronRepository();
            var borrowRepo = new InMemoryBorrowRepository();

            var bookA = new Book("978-1111111111", "A", "Auth", "Cat", 1);
            bookRepo.Add(bookA);
            await persistence.SaveAsync(bookRepo.GetAll(), bookRepo.GetAllCopies(), patronRepo.GetAll(), borrowRepo.GetAll());

            var bookB = new Book("978-2222222222", "B", "AuthB", "CatB", 1);
            bookRepo.Add(bookB);
            await persistence.SaveAsync(bookRepo.GetAll(), bookRepo.GetAllCopies(), patronRepo.GetAll(), borrowRepo.GetAll());

            var reloaded = new LibraryPersistenceService(new JsonFileDataStore<LibrarySnapshot>(tempFile));
            var bookRepo2 = new InMemoryBookRepository();
            var patronRepo2 = new InMemoryPatronRepository();
            var borrowRepo2 = new InMemoryBorrowRepository();

            var snapshot = await reloaded.LoadAsync();
            reloaded.RestoreState(snapshot!, patronRepo2, bookRepo2, borrowRepo2);

            Assert.Equal(2, bookRepo2.GetAll().Count);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void Restore_MissingBook_ThrowsInvalidOperationException()
    {
        var snapshot = new LibrarySnapshot(
            new List<BookDto>(),
            new List<CopyDto> { new CopyDto("C-MISSING", "MISSING-ISBN", CopyStatus.Available) },
            new List<PatronDto> { new PatronDto("P1", "Name", "e@e.com", "+1", DateTime.UtcNow) },
            new List<BorrowRecordDto>()
        );

        var persistence = new LibraryPersistenceService(new JsonFileDataStore<LibrarySnapshot>(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json")));
        var bookRepo = new InMemoryBookRepository();
        var patronRepo = new InMemoryPatronRepository();
        var borrowRepo = new InMemoryBorrowRepository();

        Assert.Throws<InvalidOperationException>(() => persistence.RestoreState(snapshot, patronRepo, bookRepo, borrowRepo));
    }
}
