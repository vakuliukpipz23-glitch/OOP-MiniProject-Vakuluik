using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Application;
using LibraryManagementSystem.Domain;
using LibraryManagementSystem.Infrastructure;
using Xunit;

namespace LibraryManagementSystem.Tests
{
    public class Lab36_IntegrationTests
    {
        [Fact]
        public async Task JsonFileDataStore_SaveAndLoad_PersistsSnapshot()
        {
            var file = Path.Combine(Path.GetTempPath(), $"lab36_snapshot_{Guid.NewGuid()}.json");
            try
            {
                var store = new JsonFileDataStore<LibrarySnapshot>(file);
                var snapshot = new LibrarySnapshot(new(), new(), new(), new());
                await store.SaveAsync(new[] { snapshot });

                var loaded = (await store.LoadAsync()).FirstOrDefault();
                Assert.NotNull(loaded);
                Assert.Empty(loaded!.Books);
                Assert.Empty(loaded.Copies);
                Assert.Empty(loaded.Patrons);
                Assert.Empty(loaded.BorrowRecords);
            }
            finally
            {
                if (File.Exists(file)) File.Delete(file);
            }
        }

        [Fact]
        public async Task JsonFileDataStore_Load_CorruptedFile_ThrowsInvalidOperationException()
        {
            var file = Path.Combine(Path.GetTempPath(), $"lab36_corrupt_{Guid.NewGuid()}.json");
            try
            {
                await File.WriteAllTextAsync(file, "not-json-data");
                var store = new JsonFileDataStore<LibrarySnapshot>(file);
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await store.LoadAsync());
            }
            finally
            {
                if (File.Exists(file)) File.Delete(file);
            }
        }

        [Fact]
        public async Task LibraryPersistenceService_SaveAndRestore_CanRestoreState()
        {
            var file = Path.Combine(Path.GetTempPath(), $"lab36_restore_{Guid.NewGuid()}.json");
            try
            {
                var bookRepo = new InMemoryBookRepository();
                var patronRepo = new InMemoryPatronRepository();
                var borrowRepo = new InMemoryBorrowRepository();
                var service = new LibraryPersistenceService(new JsonFileDataStore<LibrarySnapshot>(file));

                var book = new Book("9780130000060", "Title", "Author", "Category", 1);
                var copy = new Copy("C300", book);
                var patron = new Patron("P300", "Name", "n@e.com", "+123");
                bookRepo.Add(book);
                bookRepo.AddCopy(copy);
                patronRepo.Add(patron);
                borrowRepo.Add(new BorrowRecord("BR300", patron, copy, new OverduePolicy()));

                await service.SaveAsync(bookRepo.GetAll(), bookRepo.GetAllCopies(), patronRepo.GetAll(), borrowRepo.GetAll());

                var loaded = (await new JsonFileDataStore<LibrarySnapshot>(file).LoadAsync()).FirstOrDefault();
                Assert.NotNull(loaded);

                var restoreBooks = new InMemoryBookRepository();
                var restorePatrons = new InMemoryPatronRepository();
                var restoreBorrows = new InMemoryBorrowRepository();
                service.RestoreState(loaded!, restorePatrons, restoreBooks, restoreBorrows);

                Assert.NotEmpty(restoreBooks.GetAll());
                Assert.NotEmpty(restorePatrons.GetAll());
                Assert.NotEmpty(restoreBorrows.GetAll());
            }
            finally
            {
                if (File.Exists(file)) File.Delete(file);
            }
        }

        [Fact]
        public void LibraryPersistenceService_Restore_MissingBook_ThrowsInvalidOperationException()
        {
            var service = new LibraryPersistenceService(new JsonFileDataStore<LibrarySnapshot>(Path.Combine(Path.GetTempPath(), "lab36_tmp.json")));
            var snapshot = new LibrarySnapshot(
                new[] { new BookDto("9780130000061", "Title", "Author", "Category", 1) }.ToList(),
                new[] { new CopyDto("C301", "9780130000999", CopyStatus.Borrowed) }.ToList(),
                new[] { new PatronDto("P301", "Name", "n@e.com", "+123", DateTime.UtcNow) }.ToList(),
                new[] { new BorrowRecordDto("BR301", "P301", "C301", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), null, 0m) }.ToList()
            );

            var restoreBooks = new InMemoryBookRepository();
            var restorePatrons = new InMemoryPatronRepository();
            var restoreBorrows = new InMemoryBorrowRepository();

            Assert.Throws<InvalidOperationException>(() => service.RestoreState(snapshot, restorePatrons, restoreBooks, restoreBorrows));
        }

        [Fact]
        public void LibraryPersistenceService_Restore_MissingPatron_ThrowsInvalidOperationException()
        {
            var service = new LibraryPersistenceService(new JsonFileDataStore<LibrarySnapshot>(Path.Combine(Path.GetTempPath(), "lab36_tmp2.json")));
            var snapshot = new LibrarySnapshot(
                new[] { new BookDto("9780130000062", "Title", "Author", "Category", 1) }.ToList(),
                new[] { new CopyDto("C302", "9780130000062", CopyStatus.Available) }.ToList(),
                new System.Collections.Generic.List<PatronDto>(),
                new[] { new BorrowRecordDto("BR302", "P-MISSING", "C302", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), null, 0m) }.ToList()
            );

            var restoreBooks = new InMemoryBookRepository();
            var restorePatrons = new InMemoryPatronRepository();
            var restoreBorrows = new InMemoryBorrowRepository();

            Assert.Throws<InvalidOperationException>(() => service.RestoreState(snapshot, restorePatrons, restoreBooks, restoreBorrows));
        }

        [Fact]
        public async Task Persistence_SaveSequential_Succeeds()
        {
            var file = Path.Combine(Path.GetTempPath(), $"lab36_sequential_{Guid.NewGuid()}.json");
            try
            {
                var store = new JsonFileDataStore<LibrarySnapshot>(file);
                for (var i = 0; i < 3; i++)
                {
                    await store.SaveAsync(new[] { new LibrarySnapshot(new(), new(), new(), new()) });
                    var loaded = (await store.LoadAsync()).FirstOrDefault();
                    Assert.NotNull(loaded);
                }
            }
            finally
            {
                if (File.Exists(file)) File.Delete(file);
            }
        }

        [Fact]
        public async Task SaveAndReload_ThenPerformOperation_AfterRestore_Works()
        {
            var file = Path.Combine(Path.GetTempPath(), $"lab36_endtoend_{Guid.NewGuid()}.json");
            try
            {
                var bookRepo = new InMemoryBookRepository();
                var patronRepo = new InMemoryPatronRepository();
                var borrowRepo = new InMemoryBorrowRepository();
                var service = new LibraryPersistenceService(new JsonFileDataStore<LibrarySnapshot>(file));

                var book = new Book("9780130000063", "Title", "Author", "Category", 1);
                var copy = new Copy("C303", book);
                var patron = new Patron("P303", "Name", "n@e.com", "+123");
                bookRepo.Add(book);
                bookRepo.AddCopy(copy);
                patronRepo.Add(patron);
                borrowRepo.Add(new BorrowRecord("BR303", patron, copy, new OverduePolicy()));

                await service.SaveAsync(bookRepo.GetAll(), bookRepo.GetAllCopies(), patronRepo.GetAll(), borrowRepo.GetAll());

                var loaded = (await new JsonFileDataStore<LibrarySnapshot>(file).LoadAsync()).FirstOrDefault();
                Assert.NotNull(loaded);

                var restoreBooks = new InMemoryBookRepository();
                var restorePatrons = new InMemoryPatronRepository();
                var restoreBorrows = new InMemoryBorrowRepository();
                service.RestoreState(loaded!, restorePatrons, restoreBooks, restoreBorrows);

                var borrowService = new BorrowService(restoreBorrows, restorePatrons, restoreBooks);
                var records = borrowService.GetAllBorrows();
                Assert.NotEmpty(records);
            }
            finally
            {
                if (File.Exists(file)) File.Delete(file);
            }
        }
    }
}
