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
        Assert.Throws<EntityNotFoundException>(() =>
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
        Assert.Throws<BusinessRuleViolationException>(() =>
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
        Assert.Throws<BusinessRuleViolationException>(() =>
            service.ReturnBook(borrowRecord.BorrowId));
    }

    [Fact]
    public void BorrowService_BorrowBook_WithOutstandingDebt_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var patronRepo = new InMemoryPatronRepository();
        var bookRepo = new InMemoryBookRepository();
        var borrowRepo = new InMemoryBorrowRepository();
        var service = new BorrowService(borrowRepo, patronRepo, bookRepo, new OverduePolicy(feePerDay: 0.50m, gracePeriodDays: 0, maxBorrowDays: 1));

        var patron = new Patron("P001", "John Doe", "john@example.com", "+1234567890");
        var book = new Book("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 2);
        var copy1 = new Copy("COPY-001", book);
        var copy2 = new Copy("COPY-002", book);

        patronRepo.Add(patron);
        bookRepo.Add(book);
        bookRepo.AddCopy(copy1);
        bookRepo.AddCopy(copy2);

        var overdueBorrow = BorrowRecord.Rehydrate(
            "B001",
            patron,
            copy1,
            DateTime.UtcNow.AddDays(-14),
            DateTime.UtcNow.AddDays(-7),
            null,
            3.50m);

        borrowRepo.Add(overdueBorrow);

        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() =>
            service.BorrowBook("P001", "978-0134685991"));
    }

    [Fact]
    public void BorrowService_ReturnBook_NonexistentBorrow_ThrowsEntityNotFoundException()
    {
        // Arrange
        var service = CreateBorrowService();

        // Act & Assert
        Assert.Throws<EntityNotFoundException>(() =>
            service.ReturnBook("MISSING-BORROW"));
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

    [Fact]
    public void PatronService_UpdatePatronContact_NonexistentPatron_ThrowsEntityNotFoundException()
    {
        // Arrange
        var repo = new InMemoryPatronRepository();
        var service = new PatronService(repo);

        // Act & Assert
        Assert.Throws<EntityNotFoundException>(() =>
            service.UpdatePatronContact("P999", "new@example.com", "+1234567890"));
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
    public void BookService_RegisterBook_DuplicateISBN_ThrowsDuplicateEntityException()
    {
        // Arrange
        var repo = new InMemoryBookRepository();
        var service = new BookService(repo);

        service.RegisterBook("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 3);

        // Act & Assert
        Assert.Throws<DuplicateEntityException>(() =>
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
