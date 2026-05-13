using LibraryManagementSystem.Application;
using LibraryManagementSystem.Domain;
using LibraryManagementSystem.Infrastructure;

var patronRepo = new InMemoryPatronRepository();
var bookRepo = new InMemoryBookRepository();
var borrowRepo = new InMemoryBorrowRepository();

var patronService = new PatronService(patronRepo);
var bookService = new BookService(bookRepo);
var borrowService = new BorrowService(borrowRepo, patronRepo, bookRepo);

InitializeWithSampleData(bookService);

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
    Console.WriteLine("8. Exit");
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
                Console.WriteLine("Thank you for using Library Management System!");
                return;
            default:
                Console.WriteLine("Invalid option. Please try again.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
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
        Console.WriteLine($"ID: {patron.PatronId} | Name: {patron.Name} | Email: {patron.Email}");
    }
}

static void RegisterBook(BookService bookService)
{
    Console.Write("Enter ISBN: ");
    string? isbn = Console.ReadLine();

    Console.Write("Enter title: ");
    string? title = Console.ReadLine();

    Console.Write("Enter author: ");
    string? author = Console.ReadLine();

    Console.Write("Enter category: ");
    string? category = Console.ReadLine();

    Console.Write("Enter number of copies: ");
    if (!int.TryParse(Console.ReadLine(), out int copies) || copies <= 0)
    {
        Console.WriteLine("Invalid number of copies.");
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
        Console.WriteLine($"ISBN: {book.ISBN} | {book.Title} by {book.Author} | Category: {book.Category} | Available: {available}");
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
    Console.WriteLine("Book borrowed successfully!");
    Console.WriteLine($"Patron: {patron.Name}");
    Console.WriteLine($"Book: {book.Title} by {book.Author}");
    Console.WriteLine($"Borrow ID: {borrowRecord.BorrowId}");
    Console.WriteLine($"Borrow Date: {borrowRecord.BorrowDate:yyyy-MM-dd}");
    Console.WriteLine($"Due Date: {borrowRecord.DueDate:yyyy-MM-dd}");
    Console.ResetColor();
}

static void ReturnBook(BorrowService borrowService)
{
    Console.Write("Enter borrow ID: ");
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
    Console.WriteLine($"Book: {returnedRecord.Copy.Book.Title}");
    Console.WriteLine($"Patron: {returnedRecord.Patron.Name}");
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

        Console.WriteLine($"Book: {record.Copy.Book.Title}");
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

static void InitializeWithSampleData(BookService bookService)
{
    try
    {
        bookService.RegisterBook("978-0134685991", "Clean Code", "Robert C. Martin", "Programming", 3);
        bookService.RegisterBook("978-0201633610", "Design Patterns", "Gang of Four", "Programming", 2);
        bookService.RegisterBook("978-0201616224", "Refactoring", "Martin Fowler", "Programming", 2);
        Console.WriteLine("Sample books initialized in the system.\n");
    }
    catch
    {
        // Books already initialized, ignore
    }
}
