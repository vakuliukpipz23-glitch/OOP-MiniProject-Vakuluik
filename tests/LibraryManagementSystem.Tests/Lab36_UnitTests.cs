using System;
using System.Collections.Generic;
using System.Linq;
using LibraryManagementSystem.Application;
using LibraryManagementSystem.Domain;
using LibraryManagementSystem.Infrastructure;
using Xunit;

namespace LibraryManagementSystem.Tests
{
    public class Lab36_UnitTests
    {
        [Fact]
        public void Book_InvalidIsbn_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Book("123", "Title", "Author", "Category", 1));
        }

        [Fact]
        public void Book_InvalidTotalCopies_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Book("9780130000040", "Title", "Author", "Category", 0));
        }

        [Fact]
        public void Book_UpdateCategory_ValidValue_ChangesCategory()
        {
            var book = new Book("9780130000041", "Title", "Author", "Old", 1);
            book.UpdateCategory("NewCategory");
            Assert.Equal("NewCategory", book.Category);
        }

        [Fact]
        public void Book_UpdateCategory_EmptyValue_ThrowsArgumentException()
        {
            var book = new Book("9780130000042", "Title", "Author", "Old", 1);
            Assert.Throws<ArgumentException>(() => book.UpdateCategory(""));
        }

        [Fact]
        public void BookService_RegisterBook_ValidData_CreatesBookAndCopies()
        {
            var repo = new InMemoryBookRepository();
            var service = new BookService(repo);

            var book = service.RegisterBook("9780130000043", "Title", "Author", "Category", 2);

            Assert.NotNull(book);
            Assert.Equal(2, book.TotalCopies);
            Assert.Equal(2, service.GetCopies(book.ISBN).Count);
        }

        [Fact]
        public void BookService_RegisterBook_DuplicateISBN_ThrowsException()
        {
            var repo = new InMemoryBookRepository();
            var service = new BookService(repo);
            service.RegisterBook("9780130000044", "Title", "Author", "Category", 1);

            Assert.Throws<InvalidOperationException>(() =>
                service.RegisterBook("9780130000044", "Title 2", "Author", "Category", 1));
        }

        [Fact]
        public void BookService_GetAvailableCopiesCount_ReturnsCorrectCount()
        {
            var repo = new InMemoryBookRepository();
            var service = new BookService(repo);
            service.RegisterBook("9780130000045", "Title", "Author", "Category", 3);

            Assert.Equal(3, service.GetAvailableCopiesCount("9780130000045"));
        }

        [Fact]
        public void BookService_GetAvailableBooks_ReturnsBooksWithAvailableCopies()
        {
            var repo = new InMemoryBookRepository();
            var service = new BookService(repo);
            service.RegisterBook("9780130000046", "Title A", "Author", "Category", 1);
            service.RegisterBook("9780130000047", "Title B", "Author", "Category", 1);

            var available = service.GetAvailableBooks();
            Assert.Equal(2, available.Count);
        }

        [Fact]
        public void PatronService_RegisterPatron_InvalidEmail_ThrowsArgumentException()
        {
            var repo = new InMemoryPatronRepository();
            var service = new PatronService(repo);

            Assert.Throws<ArgumentException>(() => service.RegisterPatron("Name", "bad-email", "+123"));
        }

        [Fact]
        public void PatronService_UpdatePatronContact_ValidData_UpdatesContact()
        {
            var repo = new InMemoryPatronRepository();
            var service = new PatronService(repo);
            var patron = service.RegisterPatron("Name", "n@e.com", "+123");

            service.UpdatePatronContact(patron.PatronId, "new@e.com", "+456");
            var updated = service.GetPatron(patron.PatronId);

            Assert.Equal("new@e.com", updated!.Email);
            Assert.Equal("+456", updated.Phone);
        }

        [Fact]
        public void PatronService_GetPatronDebts_NoBorrows_ReturnsZero()
        {
            var repo = new InMemoryPatronRepository();
            var service = new PatronService(repo);
            var patron = service.RegisterPatron("Name", "n@e.com", "+123");

            Assert.Equal(0m, service.GetPatronDebts(patron.PatronId));
        }

        [Fact]
        public void BorrowService_BorrowBook_InvalidPatron_ThrowsException()
        {
            var repo = new InMemoryPatronRepository();
            var bookRepo = new InMemoryBookRepository();
            var borrowRepo = new InMemoryBorrowRepository();
            var service = new BorrowService(borrowRepo, repo, bookRepo);

            Assert.Throws<InvalidOperationException>(() => service.BorrowBook("missing", "9780130000048"));
        }

        [Fact]
        public void BorrowService_BorrowBook_NoAvailableCopies_ThrowsException()
        {
            var patronRepo = new InMemoryPatronRepository();
            var bookRepo = new InMemoryBookRepository();
            var borrowRepo = new InMemoryBorrowRepository();
            var service = new BorrowService(borrowRepo, patronRepo, bookRepo);
            var patron = new Patron("P200", "Name", "n@e.com", "+123");
            var book = new Book("9780130000048", "Title", "Author", "Category", 1);
            var copy = new Copy("C200", book);
            copy.MarkAsBorrowed();
            patronRepo.Add(patron);
            bookRepo.Add(book);
            bookRepo.AddCopy(copy);

            Assert.Throws<InvalidOperationException>(() => service.BorrowBook(patron.PatronId, book.ISBN));
        }

        [Fact]
        public void BorrowService_ReturnBook_AlreadyReturned_ThrowsException()
        {
            var patronRepo = new InMemoryPatronRepository();
            var bookRepo = new InMemoryBookRepository();
            var borrowRepo = new InMemoryBorrowRepository();
            var service = new BorrowService(borrowRepo, patronRepo, bookRepo);
            var patron = new Patron("P201", "Name", "n@e.com", "+123");
            var book = new Book("9780130000049", "Title", "Author", "Category", 1);
            var copy = new Copy("C201", book);
            patronRepo.Add(patron);
            bookRepo.Add(book);
            bookRepo.AddCopy(copy);
            var record = service.BorrowBook(patron.PatronId, book.ISBN);
            service.ReturnBook(record.BorrowId);

            Assert.Throws<InvalidOperationException>(() => service.ReturnBook(record.BorrowId));
        }

        [Fact]
        public void BorrowService_GetPatronOverdueBorrows_WhenOverdue_ReturnsList()
        {
            var patronRepo = new InMemoryPatronRepository();
            var bookRepo = new InMemoryBookRepository();
            var borrowRepo = new InMemoryBorrowRepository();
            var service = new BorrowService(borrowRepo, patronRepo, bookRepo);
            var book = new Book("9780130000050", "Title", "Author", "Category", 1);
            var copy = new Copy("C202", book);
            var patron = new Patron("P202", "Name", "n@e.com", "+123");
            var past = DateTime.UtcNow.AddDays(-40);
            var due = past.AddDays(30);
            bookRepo.Add(book);
            bookRepo.AddCopy(copy);
            patronRepo.Add(patron);
            borrowRepo.Add(BorrowRecord.Rehydrate("R202", patron, copy, past, due, null, 0));

            var overdue = service.GetPatronOverdueBorrows(patron.PatronId);
            Assert.Single(overdue);
        }

        [Fact]
        public void BorrowService_GetPatronTotalDebts_ReturnsOverdueFee()
        {
            var patronRepo = new InMemoryPatronRepository();
            var bookRepo = new InMemoryBookRepository();
            var borrowRepo = new InMemoryBorrowRepository();
            var service = new BorrowService(borrowRepo, patronRepo, bookRepo);
            var book = new Book("9780130000051", "Title", "Author", "Category", 1);
            var copy = new Copy("C203", book);
            var patron = new Patron("P203", "Name", "n@e.com", "+123");
            var past = DateTime.UtcNow.AddDays(-40);
            var due = past.AddDays(30);
            bookRepo.Add(book);
            bookRepo.AddCopy(copy);
            patronRepo.Add(patron);
            borrowRepo.Add(BorrowRecord.Rehydrate("R203", patron, copy, past, due, null, 0));

            var debt = service.GetPatronTotalDebts(patron.PatronId);
            Assert.True(debt > 0);
        }

        [Fact]
        public void QueryService_SearchBooks_ByTitle_ReturnsCorrectBook()
        {
            var bookRepo = new InMemoryBookRepository();
            var borrowRepo = new InMemoryBorrowRepository();
            var service = new LibraryQueryService(bookRepo, borrowRepo);
            bookRepo.Add(new Book("9780130000052", "UniqueTitle", "Author", "Category", 1));

            var result = service.SearchBooks("UniqueTitle", null, null);
            Assert.Single(result);
        }

        [Fact]
        public void QueryService_GetCategories_ReturnsUniqueCategories()
        {
            var bookRepo = new InMemoryBookRepository();
            var borrowRepo = new InMemoryBorrowRepository();
            var service = new LibraryQueryService(bookRepo, borrowRepo);
            bookRepo.Add(new Book("9780130000053", "Title", "Author", "Sci", 1));
            bookRepo.Add(new Book("9780130000054", "Title2", "Author", "sci", 1));

            var categories = service.GetCategories();
            Assert.Single(categories);
        }

        [Fact]
        public void QueryService_GetTopBorrowedBooks_LimitsResults()
        {
            var bookRepo = new InMemoryBookRepository();
            var borrowRepo = new InMemoryBorrowRepository();
            var service = new LibraryQueryService(bookRepo, borrowRepo);
            var book = new Book("9780130000055", "Popular", "Author", "Category", 2);
            var copy1 = new Copy("C204", book);
            var copy2 = new Copy("C205", book);
            bookRepo.Add(book);
            bookRepo.AddCopy(copy1);
            bookRepo.AddCopy(copy2);
            var patron = new Patron("P204", "Name", "n@e.com", "+123");
            borrowRepo.Add(new BorrowRecord("R204-1", patron, copy1, new OverduePolicy()));
            borrowRepo.Add(new BorrowRecord("R204-2", patron, copy2, new OverduePolicy()));

            var top = service.GetTopBorrowedBooks(1);
            Assert.Single(top);
            Assert.Equal(book.ISBN, top[0].Book.ISBN);
        }

        [Fact]
        public void BookService_GetCopies_ReturnsAllCopies()
        {
            var repo = new InMemoryBookRepository();
            var service = new BookService(repo);
            service.RegisterBook("9780130000056", "Title", "Author", "Category", 2);

            var copies = service.GetCopies("9780130000056");
            Assert.Equal(2, copies.Count);
        }

        [Fact]
        public void PatronService_GetPatron_ExistingPatron_ReturnsPatron()
        {
            var repo = new InMemoryPatronRepository();
            var service = new PatronService(repo);
            var patron = service.RegisterPatron("Name", "n@e.com", "+123");

            var fetched = service.GetPatron(patron.PatronId);
            Assert.NotNull(fetched);
            Assert.Equal(patron.PatronId, fetched!.PatronId);
        }
    }
}
