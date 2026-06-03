using System.IO;
using LibraryManagementSystem.Application;
using LibraryManagementSystem.Domain;
using LibraryManagementSystem.Infrastructure;
using Xunit;

namespace LibraryManagementSystem.Tests;

public class BorrowServiceTests
{
    private BorrowService CreateBorrowService()
    {
        var patronRepo = new InMemoryPatronRepository();
        var bookRepo = new InMemoryBookRepository();
        var borrowRepo = new InMemoryBorrowRepository();

        return new BorrowService(borrowRepo, patronRepo, bookRepo);
    }

    [Fact]
    public void BorrowService_BorrowBook_ValidPatronAndBook_CreatesBorrowRecord()
    {
        // Arrange
        var patronRepo = new InMemoryPatronRepository();
        var bookRepo = new InMemoryBookRepository();
        var borrowRepo = new InMemoryBorrowRepository();
        var service = new BorrowService(borrowRepo, patronRepo, bookRepo);

        var patron = new Patron("P001", "John Doe", "john@example.com", "+1234567890");
        var book = new Book("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 1);
        var copy = new Copy("COPY-001", book);

        patronRepo.Add(patron);
        bookRepo.Add(book);
        bookRepo.AddCopy(copy);

        // Act
        var borrowRecord = service.BorrowBook("P001", "978-0134685991");

        // Assert
        Assert.NotNull(borrowRecord);
        Assert.Equal(patron.PatronId, borrowRecord.Patron.PatronId);
        Assert.Equal(book.ISBN, borrowRecord.Copy.Book.ISBN);
        Assert.Null(borrowRecord.ReturnDate);
    }

    [Fact]
    public void BorrowService_BorrowBook_InvalidPatron_ThrowsException()
    {
        // Arrange
        var service = CreateBorrowService();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            service.BorrowBook("NONEXISTENT", "978-0134685991"));
    }

    [Fact]
    public void BorrowService_BorrowBook_NoAvailableCopies_ThrowsException()
    {
        // Arrange
        var patronRepo = new InMemoryPatronRepository();
        var bookRepo = new InMemoryBookRepository();
        var borrowRepo = new InMemoryBorrowRepository();
        var service = new BorrowService(borrowRepo, patronRepo, bookRepo);

        var patron = new Patron("P001", "John Doe", "john@example.com", "+1234567890");
        var book = new Book("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 1);
        var copy = new Copy("COPY-001", book);
        copy.MarkAsBorrowed();

        patronRepo.Add(patron);
        bookRepo.Add(book);
        bookRepo.AddCopy(copy);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            service.BorrowBook("P001", "978-0134685991"));
    }

    [Fact]
    public void BorrowService_ReturnBook_ValidBorrow_UpdatesCopyStatus()
    {
        // Arrange
        var patronRepo = new InMemoryPatronRepository();
        var bookRepo = new InMemoryBookRepository();
        var borrowRepo = new InMemoryBorrowRepository();
        var service = new BorrowService(borrowRepo, patronRepo, bookRepo);

        var patron = new Patron("P001", "John Doe", "john@example.com", "+1234567890");
        var book = new Book("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 1);
        var copy = new Copy("COPY-001", book);

        patronRepo.Add(patron);
        bookRepo.Add(book);
        bookRepo.AddCopy(copy);

        var borrowRecord = service.BorrowBook("P001", "978-0134685991");

        // Act
        var returnedRecord = service.ReturnBook(borrowRecord.BorrowId);

        // Assert
        Assert.NotNull(returnedRecord.ReturnDate);
        Assert.Equal(CopyStatus.Available, copy.Status);
    }

    [Fact]
    public void BorrowService_ReturnBook_AlreadyReturned_ThrowsException()
    {
        // Arrange
        var patronRepo = new InMemoryPatronRepository();
        var bookRepo = new InMemoryBookRepository();
        var borrowRepo = new InMemoryBorrowRepository();
        var service = new BorrowService(borrowRepo, patronRepo, bookRepo);

        var patron = new Patron("P001", "John Doe", "john@example.com", "+1234567890");
        var book = new Book("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 1);
        var copy = new Copy("COPY-001", book);

        patronRepo.Add(patron);
        bookRepo.Add(book);
        bookRepo.AddCopy(copy);

        var borrowRecord = service.BorrowBook("P001", "978-0134685991");
        service.ReturnBook(borrowRecord.BorrowId);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            service.ReturnBook(borrowRecord.BorrowId));
    }

    [Fact]
    public void BorrowService_GetPatronActiveBorrows_ReturnsOnlyUnreturned()
    {
        // Arrange
        var patronRepo = new InMemoryPatronRepository();
        var bookRepo = new InMemoryBookRepository();
        var borrowRepo = new InMemoryBorrowRepository();
        var service = new BorrowService(borrowRepo, patronRepo, bookRepo);

        var patron = new Patron("P001", "John Doe", "john@example.com", "+1234567890");
        var book1 = new Book("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 2);
        var copy1 = new Copy("COPY-001", book1);
        var copy2 = new Copy("COPY-002", book1);

        patronRepo.Add(patron);
        bookRepo.Add(book1);
        bookRepo.AddCopy(copy1);
        bookRepo.AddCopy(copy2);

        var borrow1 = service.BorrowBook("P001", "978-0134685991");
        var borrow2 = service.BorrowBook("P001", "978-0134685991");

        service.ReturnBook(borrow1.BorrowId); // Return first book

        // Act
        var activeBorrows = service.GetPatronActiveBorrows("P001");

        // Assert
        Assert.Single(activeBorrows);
        Assert.Equal(borrow2.BorrowId, activeBorrows[0].BorrowId);
    }
}

public class PatronServiceTests
{
    [Fact]
    public void PatronService_RegisterPatron_ValidData_CreatesPatron()
    {
        // Arrange
        var repo = new InMemoryPatronRepository();
        var service = new PatronService(repo);

        // Act
        var patron = service.RegisterPatron("John Doe", "john@example.com", "+1234567890");

        // Assert
        Assert.NotNull(patron);
        Assert.NotEmpty(patron.PatronId);
        Assert.Equal("John Doe", patron.Name);
        Assert.Equal("john@example.com", patron.Email);
    }

    [Fact]
    public void PatronService_RegisterPatron_InvalidEmail_ThrowsException()
    {
        // Arrange
        var repo = new InMemoryPatronRepository();
        var service = new PatronService(repo);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            service.RegisterPatron("John Doe", "invalid-email", "+1234567890"));
    }

    [Fact]
    public void PatronService_GetPatron_ExistingPatron_ReturnsPatron()
    {
        // Arrange
        var repo = new InMemoryPatronRepository();
        var service = new PatronService(repo);

        var registered = service.RegisterPatron("John Doe", "john@example.com", "+1234567890");

        // Act
        var retrieved = service.GetPatron(registered.PatronId);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(registered.PatronId, retrieved.PatronId);
    }
}

public class BookServiceTests
{
    [Fact]
    public void BookService_RegisterBook_ValidData_CreatesBook()
    {
        // Arrange
        var repo = new InMemoryBookRepository();
        var service = new BookService(repo);

        // Act
        var book = service.RegisterBook("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 3);

        // Assert
        Assert.NotNull(book);
        Assert.Equal("978-0134685991", book.ISBN);
        Assert.Equal("Clean Code", book.Title);
        Assert.Equal(3, book.TotalCopies);
    }

    [Fact]
    public void BookService_RegisterBook_DuplicateISBN_ThrowsException()
    {
        // Arrange
        var repo = new InMemoryBookRepository();
        var service = new BookService(repo);

        service.RegisterBook("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 3);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            service.RegisterBook("978-0134685991", "Different Title", "Different Author", "Programming", 1));
    }

    [Fact]
    public void BookService_GetAvailableBooks_ReturnsOnlyBooksWithAvailableCopies()
    {
        // Arrange
        var repo = new InMemoryBookRepository();
        var service = new BookService(repo);

        service.RegisterBook("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 2);
        service.RegisterBook("978-0201633610", "Design Patterns", "Gang of Four", "Programming", 1);

        // Act
        var available = service.GetAvailableBooks();

        // Assert
        Assert.Equal(2, available.Count);
    }

    [Fact]
    public void BookService_GetAvailableCopiesCount_ReturnsCorrectCount()
    {
        // Arrange
        var repo = new InMemoryBookRepository();
        var service = new BookService(repo);

        service.RegisterBook("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 3);

        // Act
        int count = service.GetAvailableCopiesCount("978-0134685991");

        // Assert
        Assert.Equal(3, count);
    }
}

public class QueryServiceTests
{
    [Fact]
    public void LibraryQueryService_GetPatronsWithOverdueDebt_ReturnsOverduePatrons()
    {
        // Arrange
        var bookRepo = new InMemoryBookRepository();
        var borrowRepo = new InMemoryBorrowRepository();
        var queryService = new LibraryQueryService(bookRepo, borrowRepo);

        var book = new Book("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 1);
        var copy = new Copy("COPY-001", book);
        var patron = new Patron("P001", "John Doe", "john@example.com", "+1234567890");
        var pastBorrowDate = DateTime.UtcNow.AddDays(-40);
        var dueDate = pastBorrowDate.AddDays(30);

        bookRepo.Add(book);
        bookRepo.AddCopy(copy);

        var overdueBorrow = BorrowRecord.Rehydrate("B001", patron, copy, pastBorrowDate, dueDate, null, 0);
        borrowRepo.Add(overdueBorrow);

        // Act
        var overduePatrons = queryService.GetPatronsWithOverdueDebt();

        // Assert
        Assert.Single(overduePatrons);
        Assert.Equal("P001", overduePatrons[0].Patron.PatronId);
        Assert.True(overduePatrons[0].OverdueDebt > 0);
    }
}

public class BorrowServiceBusinessRulesTests
{
    [Fact]
    public void BorrowService_BorrowBook_WhenPatronHasOutstandingDebt_ThrowsException()
    {
        // Arrange
        var patronRepo = new InMemoryPatronRepository();
        var bookRepo = new InMemoryBookRepository();
        var borrowRepo = new InMemoryBorrowRepository();
        var service = new BorrowService(borrowRepo, patronRepo, bookRepo);

        var patron = new Patron("P001", "John Doe", "john@example.com", "+1234567890");
        var book = new Book("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 1);
        var copy = new Copy("COPY-001", book);
        var pastBorrowDate = DateTime.UtcNow.AddDays(-40);
        var dueDate = pastBorrowDate.AddDays(30);

        bookRepo.Add(book);
        bookRepo.AddCopy(copy);
        patronRepo.Add(patron);
        borrowRepo.Add(BorrowRecord.Rehydrate("B001", patron, copy, pastBorrowDate, dueDate, null, 0));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => service.BorrowBook("P001", "978-0134685991"));
    }
}

public class PersistenceTests
{
    [Fact]
    public async Task JsonFileDataStore_SaveAsyncAndLoadAsync_PersistsSnapshot()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"library-snapshot-{Guid.NewGuid()}.json");

        try
        {
            var dataStore = new JsonFileDataStore<LibrarySnapshot>(tempFile);
            var snapshot = new LibrarySnapshot(
                new List<BookDto>(),
                new List<CopyDto>(),
                new List<PatronDto>(),
                new List<BorrowRecordDto>());

            await dataStore.SaveAsync(new[] { snapshot });
            var loaded = await dataStore.LoadAsync();

            Assert.Single(loaded);
            Assert.Empty(loaded.First().Books);
            Assert.Empty(loaded.First().Copies);
            Assert.Empty(loaded.First().Patrons);
            Assert.Empty(loaded.First().BorrowRecords);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task LibraryPersistenceService_SaveAndLoad_RestoresLibraryState()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"library-state-{Guid.NewGuid()}.json");

        try
        {
            var bookRepo = new InMemoryBookRepository();
            var patronRepo = new InMemoryPatronRepository();
            var borrowRepo = new InMemoryBorrowRepository();
            var persistenceService = new LibraryPersistenceService(new JsonFileDataStore<LibrarySnapshot>(tempFile));

            var book = new Book("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 1);
            var copy = new Copy("COPY-001", book);
            var patron = new Patron("P001", "John Doe", "john@example.com", "+1234567890");
            var policy = new OverduePolicy();
            var borrowRecord = new BorrowRecord("B001", patron, copy, policy);

            bookRepo.Add(book);
            bookRepo.AddCopy(copy);
            patronRepo.Add(patron);
            borrowRepo.Add(borrowRecord);

            await persistenceService.SaveAsync(bookRepo.GetAll(), bookRepo.GetAllCopies(), patronRepo.GetAll(), borrowRepo.GetAll());

            var restoreBookRepo = new InMemoryBookRepository();
            var restorePatronRepo = new InMemoryPatronRepository();
            var restoreBorrowRepo = new InMemoryBorrowRepository();

            var loadedSnapshot = (await new JsonFileDataStore<LibrarySnapshot>(tempFile).LoadAsync()).FirstOrDefault();
            Assert.NotNull(loadedSnapshot);

            persistenceService.RestoreState(loadedSnapshot!, restorePatronRepo, restoreBookRepo, restoreBorrowRepo);

            var restoredBorrow = restoreBorrowRepo.GetById("B001");
            Assert.NotNull(restoredBorrow);
            Assert.Equal("P001", restoredBorrow!.Patron.PatronId);
            Assert.Equal("COPY-001", restoredBorrow.Copy.CopyId);
            Assert.Equal(CopyStatus.Borrowed, restoredBorrow.Copy.Status);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
