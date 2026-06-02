using LibraryManagementSystem.Application;
using LibraryManagementSystem.Domain;
using LibraryManagementSystem.Infrastructure;

const string LibraryDataFile = "data/library.json";

var patronRepo = new InMemoryPatronRepository();
var bookRepo = new InMemoryBookRepository();
var borrowRepo = new InMemoryBorrowRepository();

var patronService = new PatronService(patronRepo);
var bookService = new BookService(bookRepo);
var borrowService = new BorrowService(borrowRepo, patronRepo, bookRepo);
var queryService = new LibraryQueryService(bookRepo, borrowRepo);

var persistenceService = new LibraryPersistenceService(new JsonFileDataStore<LibrarySnapshot>(LibraryDataFile));
await LoadLibraryStateAsync(persistenceService, patronRepo, bookRepo, borrowRepo);
EnsureSampleDataExists(bookService);

while (true)
{
    Console.WriteLine("\n=== Library Management System ===");
    Console.WriteLine("1. Register New Patron");
    Console.WriteLine("2. List All Patrons");
    Console.WriteLine("3. Register New Book");
    Console.WriteLine("4. List Available Books");
    Console.WriteLine("5. Borrow a Book");
    Console.WriteLine("6. Return a Book");
    Console.WriteLine("7. View Patron's Borrows");
    Console.WriteLine("8. Search Books");
    Console.WriteLine("9. Analytics & Reports");
    Console.WriteLine("10. Update Patron Contact");
    Console.WriteLine("11. Update Book Category");
    Console.WriteLine("12. Save Library State");
    Console.WriteLine("13. Exit");
    Console.Write("Choose option: ");

    string? option = Console.ReadLine();

    try
    {
        switch (option)
        {
            case "1":
                RegisterPatron(patronService);
                break;
            case "2":
                ListPatrons(patronService);
                break;
            case "3":
                RegisterBook(bookService);
                break;
            case "4":
                ListAvailableBooks(bookService);
                break;
            case "5":
                BorrowBook(borrowService, patronService, bookService);
                break;
            case "6":
                ReturnBook(borrowService);
                break;
            case "7":
                ViewPatronBorrows(borrowService, patronService);
                break;
            case "8":
                SearchBooks(queryService);
                break;
            case "9":
                ShowAnalytics(queryService, patronService, bookService);
                break;
            case "10":
                UpdatePatronContact(patronService);
                break;
            case "11":
                UpdateBookCategory(bookService);
                break;
            case "12":
                await SaveLibraryStateAsync(persistenceService, bookRepo, patronRepo, borrowRepo);
                break;
            case "13":
                await SaveLibraryStateAsync(persistenceService, bookRepo, patronRepo, borrowRepo);
                Console.WriteLine("Дякуємо за використання Системи Управління Бібліотекою!");
                return;
            default:
                Console.WriteLine("Невірна опція. Спробуйте ще раз.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Помилка: {ex.Message}");
        Console.ResetColor();
    }
}

static async Task LoadLibraryStateAsync(LibraryPersistenceService persistenceService, InMemoryPatronRepository patronRepo, InMemoryBookRepository bookRepo, InMemoryBorrowRepository borrowRepo)
{
    try
    {
        var snapshot = await persistenceService.LoadAsync();
        if (snapshot is not null)
        {
            persistenceService.RestoreState(snapshot, patronRepo, bookRepo, borrowRepo);
            Console.WriteLine("Library state loaded successfully.");
        }
        else
        {
            Console.WriteLine("No saved library state found. Starting with fresh state.");
        }
    }
    catch
    {
        Console.WriteLine("Failed to load saved library state. Starting with fresh state.");
    }
}

static async Task SaveLibraryStateAsync(LibraryPersistenceService persistenceService, InMemoryBookRepository bookRepo, InMemoryPatronRepository patronRepo, InMemoryBorrowRepository borrowRepo)
{
    var allBooks = bookRepo.GetAll();
    var allCopies = bookRepo.GetAllCopies();
    var allPatrons = patronRepo.GetAll();
    var allBorrowRecords = borrowRepo.GetAll();

    await persistenceService.SaveAsync(allBooks, allCopies, allPatrons, allBorrowRecords);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Library state saved successfully.");
    Console.ResetColor();
}

static void EnsureSampleDataExists(BookService bookService)
{
    if (!bookService.GetAllBooks().Any())
    {
        InitializeWithSampleData(bookService);
    }
}

static void RegisterPatron(PatronService patronService)
{
    Console.Write("Enter patron name: ");
    string? name = Console.ReadLine();

    Console.Write("Enter email: ");
    string? email = Console.ReadLine();

    Console.Write("Enter phone: ");
    string? phone = Console.ReadLine();

    var patron = patronService.RegisterPatron(name!, email!, phone!);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Patron registered successfully!");
    Console.WriteLine($"Patron ID: {patron.PatronId}");
    Console.WriteLine($"Name: {patron.Name}");
    Console.ResetColor();
}

static void ListPatrons(PatronService patronService)
{
    var patrons = patronService.GetAllPatrons();
    if (!patrons.Any())
    {
        Console.WriteLine("No patrons registered yet.");
        return;
    }

    Console.WriteLine("\n=== Registered Patrons ===");
    foreach (var patron in patrons)
    {
        Console.WriteLine($"ID: {patron.PatronId} | Name: {patron.Name} | Email: {patron.Email} | Phone: {patron.Phone}");
    }
}

static void RegisterBook(BookService bookService)
{
    Console.Write("Enter ISBN: ");
    string? isbn = Console.ReadLine();

    Console.Write("Enter title: ");
    string? title = Console.ReadLine();

    Console.Write("Введіть автора: ");
    string? author = Console.ReadLine();

    Console.Write("Введіть категорію: ");
    string? category = Console.ReadLine();

    Console.Write("Введіть кількість копій: ");
    if (!int.TryParse(Console.ReadLine(), out int copies) || copies <= 0)
    {
        Console.WriteLine("Невірна кількість копій.");
        return;
    }

    var book = bookService.RegisterBook(isbn!, title!, author!, category!, copies);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Book registered successfully!");
    Console.WriteLine($"ISBN: {book.ISBN}");
    Console.WriteLine($"Title: {book.Title}");
    Console.WriteLine($"Total copies: {book.TotalCopies}");
    Console.ResetColor();
}

static void ListAvailableBooks(BookService bookService)
{
    var books = bookService.GetAvailableBooks();
    if (!books.Any())
    {
        Console.WriteLine("No available books at the moment.");
        return;
    }

    Console.WriteLine("\n=== Available Books ===");
    foreach (var book in books)
    {
        int available = bookService.GetAvailableCopiesCount(book.ISBN);
        Console.WriteLine($"ISBN: {book.ISBN} | {book.Title} автор {book.Author} | Категорія: {book.Category} | Доступно: {available}");
    }
}

static void BorrowBook(BorrowService borrowService, PatronService patronService, BookService bookService)
{
    Console.Write("Enter patron ID: ");
    string? patronId = Console.ReadLine();

    var patron = patronService.GetPatron(patronId!);
    if (patron is null)
    {
        Console.WriteLine($"Patron {patronId} not found.");
        return;
    }

    Console.Write("Enter book ISBN: ");
    string? isbn = Console.ReadLine();

    var book = bookService.FindBook(isbn!);
    if (book is null)
    {
        Console.WriteLine($"Book with ISBN {isbn} not found.");
        return;
    }

    var borrowRecord = borrowService.BorrowBook(patronId!, isbn!);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Книгу успішно позичено!");
    Console.WriteLine($"Відвідувач: {patron.Name}");
    Console.WriteLine($"Книга: {book.Title} автор {book.Author}");
    Console.WriteLine($"ID позичення: {borrowRecord.BorrowId}");
    Console.WriteLine($"Дата позичення: {borrowRecord.BorrowDate:yyyy-MM-dd}");
    Console.WriteLine($"Термін повернення: {borrowRecord.DueDate:yyyy-MM-dd}");
    Console.ResetColor();
}

static void ReturnBook(BorrowService borrowService)
{
    Console.Write("Введіть ID позичення: ");
    string? borrowId = Console.ReadLine();

    var borrowRecord = borrowService.GetBorrowRecord(borrowId!);
    if (borrowRecord is null)
    {
        Console.WriteLine($"Borrow record {borrowId} not found.");
        return;
    }

    if (borrowRecord.ReturnDate.HasValue)
    {
        Console.WriteLine("This book has already been returned.");
        return;
    }

    var returnedRecord = borrowService.ReturnBook(borrowId!);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Book returned successfully!");
    Console.WriteLine($"Книга: {returnedRecord.Copy.Book.Title}");
    Console.WriteLine($"Відвідувач: {returnedRecord.Patron.Name}");
    Console.WriteLine($"Return Date: {returnedRecord.ReturnDate:yyyy-MM-dd}");

    if (returnedRecord.IsOverdue())
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"OVERDUE! Days late: {returnedRecord.GetDaysOverdue()}");
        Console.WriteLine($"Overdue fee: {returnedRecord.OverdueFee:C}");
    }
    else
    {
        Console.WriteLine("Returned on time!");
    }

    Console.ResetColor();
}

static void ViewPatronBorrows(BorrowService borrowService, PatronService patronService)
{
    Console.Write("Enter patron ID: ");
    string? patronId = Console.ReadLine();

    var patron = patronService.GetPatron(patronId!);
    if (patron is null)
    {
        Console.WriteLine($"Patron {patronId} not found.");
        return;
    }

    var borrows = borrowService.GetPatronBorrows(patronId!);
    if (!borrows.Any())
    {
        Console.WriteLine($"Patron {patron.Name} has no borrow history.");
        return;
    }

    Console.WriteLine($"\n=== Borrow History for {patron.Name} ===");
    foreach (var record in borrows)
    {
        string status = record.ReturnDate.HasValue
            ? $"Returned on {record.ReturnDate:yyyy-MM-dd}"
            : (record.IsOverdue() ? "OVERDUE" : $"Due {record.DueDate:yyyy-MM-dd}");

        Console.WriteLine($"Книга: {record.Copy.Book.Title}");
        Console.WriteLine($"Borrowed: {record.BorrowDate:yyyy-MM-dd} | Status: {status}");
        if (record.OverdueFee > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Overdue Fee: {record.OverdueFee:C}");
            Console.ResetColor();
        }
        Console.WriteLine();
    }
}

static void SearchBooks(LibraryQueryService queryService)
{
    Console.Write("Enter search term: ");
    string? query = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(query))
    {
        Console.WriteLine("Please enter a valid search term.");
        return;
    }

    var results = queryService.SearchBooks(query!);
    if (!results.Any())
    {
        Console.WriteLine("No books found matching the search criteria.");
        return;
    }

    Console.WriteLine($"\n=== Search Results ({results.Count}) ===");
    foreach (var book in results)
    {
        Console.WriteLine($"ISBN: {book.ISBN} | {book.Title} автор {book.Author} | Категорія: {book.Category}");
    }
}

static void ShowAnalytics(LibraryQueryService queryService, PatronService patronService, BookService bookService)
{
    Console.WriteLine("\n=== Аналітика бібліотеки ===");

    var activeBorrows = queryService.GetActiveBorrowRecords();
    Console.WriteLine($"Активних позичень: {activeBorrows.Count}");

    var overdueBorrows = queryService.GetOverdueBorrows();
    Console.WriteLine($"Прострочених позичень: {overdueBorrows.Count}");

    var overduePatrons = queryService.GetPatronsWithOverdueDebt();
    Console.WriteLine($"Відвідувачів із заборгованістю: {overduePatrons.Count}");
    foreach (var (patron, debt) in overduePatrons.Take(5))
    {
        Console.WriteLine($"- {patron.Name} (ID: {patron.PatronId}) має заборгованість {debt:C}");
    }

    var topBooks = queryService.GetTopBorrowedBooks(5);
    Console.WriteLine("Топ позичуваних книг:");
    foreach (var (book, count) in topBooks)
    {
        Console.WriteLine($"- {book.Title} автор {book.Author} (ISBN: {book.ISBN}) - кількість позичень: {count}");
    }

    Console.Write("Введіть ID відвідувача для підсумку боргу (необов'язково): ");
    string? patronId = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(patronId))
    {
        var patron = patronService.GetPatron(patronId!);
        if (patron is not null)
        {
            Console.WriteLine($"Patron {patron.Name} має заборгованість {patron.GetTotalOverdueFeesOwed():C}");
        }
        else
        {
            Console.WriteLine($"Patron {patronId} not found.");
        }
    }
}

static void UpdatePatronContact(PatronService patronService)
{
    Console.Write("Enter patron ID: ");
    string? patronId = Console.ReadLine();

    Console.Write("Enter new email: ");
    string? email = Console.ReadLine();

    Console.Write("Enter new phone: ");
    string? phone = Console.ReadLine();

    patronService.UpdatePatronContact(patronId!, email!, phone!);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Patron contact updated successfully.");
    Console.ResetColor();
}

static void UpdateBookCategory(BookService bookService)
{
    Console.Write("Enter book ISBN: ");
    string? isbn = Console.ReadLine();

    Console.Write("Введіть нову категорію: ");
    string? category = Console.ReadLine();

    bookService.UpdateBookCategory(isbn!, category!);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Категорію книги оновлено успішно.");
    Console.ResetColor();
}

static void InitializeWithSampleData(BookService bookService)
{
    try
    {
        bookService.RegisterBook("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 3);
        bookService.RegisterBook("978-0201633610", "Design Patterns", "Gang of Four", "Programming", 2);
        bookService.RegisterBook("978-0201616224", "Refactoring", "Martin Fowler", "Programming", 2);
        Console.WriteLine("Початкові книги успішно додано до системи.\n");
    }
    catch
    {
        // Books already initialized, ignore
    }
}


