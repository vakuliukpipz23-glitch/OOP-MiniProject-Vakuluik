# Діаграма Класів - Система Управління Бібліотекою

```mermaid
classDiagram
    %% Domain Layer Entities
    class Book {
        -string isbn
        -string title
        -string author
        -string category
        -int totalCopies
        +Book(isbn, title, author, category, totalCopies)
        +GetAvailableCopies() int
        +ValidateISBN() void
    }

    class Copy {
        -string copyId
        -Book book
        -CopyStatus status
        +Copy(copyId, book)
        +MarkAsBorrowed() void
        +MarkAsReturned() void
        +GetStatus() CopyStatus
    }

    class CopyStatus {
        <<enumeration>>
        Available
        Borrowed
        Reserved
        Damaged
    }

    class Patron {
        -string patronId
        -string name
        -string email
        -string phone
        -DateTime registrationDate
        -List~BorrowRecord~ borrowHistory
        +Patron(name, email, phone)
        +AddBorrowRecord(record) void
        +GetActiveBorrows() List
        +ValidateContactInfo() void
    }

    class BorrowRecord {
        -string borrowId
        -Patron patron
        -Copy copy
        -DateTime borrowDate
        -DateTime dueDate
        -DateTime? returnDate
        -decimal overdueFee
        +BorrowRecord(patron, copy)
        +ReturnBook(returnDate) void
        +CalculateOverdueFee() decimal
        +IsOverdue() bool
        +GetDaysOverdue() int
    }

    class OverduePolicy {
        -decimal feePerDay
        -int gracePeriodDays
        +OverduePolicy(feePerDay, gracePeriodDays)
        +CalculateFee(daysLate) decimal
    }

    %% Application Layer Services
    class PatronService {
        -IPatronRepository patronRepo
        +PatronService(patronRepo)
        +RegisterPatron(name, email, phone) Patron
        +GetPatron(patronId) Patron
        +GetPatronBorrows(patronId) List
    }

    class BorrowService {
        -IBorrowRepository borrowRepo
        -IBookRepository bookRepo
        -IPatronRepository patronRepo
        -OverduePolicy overduePolicy
        +BorrowService(repos)
        +BorrowBook(patronId, isbn) BorrowRecord
        +ReturnBook(borrowId) BorrowRecord
        +GetPatronDebts(patronId) decimal
    }

    class BookService {
        -IBookRepository bookRepo
        +BookService(bookRepo)
        +RegisterBook(isbn, title, author, category, copies) Book
        +FindBook(isbn) Book
        +GetAvailableBooks() List
    }

    %% Infrastructure Layer Repositories
    class IPatronRepository {
        <<interface>>
        +Add(patron) void
        +GetById(id) Patron
        +GetAll() List
        +Update(patron) void
    }

    class IBookRepository {
        <<interface>>
        +Add(book) void
        +GetById(isbn) Book
        +GetAll() List
        +Update(book) void
    }

    class IBorrowRepository {
        <<interface>>
        +Add(record) void
        +GetById(id) BorrowRecord
        +GetByPatronId(patronId) List
        +Update(record) void
    }

    class InMemoryPatronRepository {
        -Dictionary~string, Patron~ patronsDb
        +Add(patron) void
        +GetById(id) Patron
        +GetAll() List
        +Update(patron) void
    }

    class InMemoryBookRepository {
        -Dictionary~string, Book~ booksDb
        -Dictionary~string, List~Copy~~ copiesDb
        +Add(book) void
        +GetById(isbn) Book
        +GetAll() List
        +Update(book) void
    }

    class InMemoryBorrowRepository {
        -Dictionary~string, BorrowRecord~ borrowsDb
        +Add(record) void
        +GetById(id) BorrowRecord
        +GetByPatronId(patronId) List
        +Update(record) void
    }

    %% Relationships
    Book "1" --> "*" Copy : contains
    Copy "1" --> "1" CopyStatus : has
    Patron "1" --> "*" BorrowRecord : makes
    Copy "1" --> "*" BorrowRecord : recorded in
    BorrowRecord "1" --> "1" OverduePolicy : uses

    %% Service to Repository relationships
    PatronService "1" --> "1" IPatronRepository : uses
    BorrowService "1" --> "1" IBorrowRepository : uses
    BorrowService "1" --> "1" IBookRepository : uses
    BorrowService "1" --> "1" IPatronRepository : uses
    BookService "1" --> "1" IBookRepository : uses

    %% Implementation relationships
    IPatronRepository <|.. InMemoryPatronRepository
    IBookRepository <|.. InMemoryBookRepository
    IBorrowRepository <|.. InMemoryBorrowRepository
```

## Пояснення Діаграми

**Шар Домену (Зелений):**
- Основні бізнес-сутності з інкапсульованими властивостями та валідацією
- `Book`, `Copy`, `Patron`, `BorrowRecord`, `OverduePolicy`
- Містить усі бізнес-правила та інваріанти

**Шар Застосунку (Синій):**
- Оркестрація бізнес-логіки
- `PatronService`, `BorrowService`, `BookService`
- Використовує ін'єкцію залежностей для доступу до репозиторіїв
- Координує між доменом та інфраструктурою

**Шар Інфраструктури (Помаранчевий):**
- Інтерфейси репозиторіїв для абстракції доступу до даних
- In-memory реалізації для Ітерації 1
- Буде розширено реалізаціями SQL в Ітерації 2

**Ключові Паттерни Проектування, що Використані:**
1. **Паттерн Репозиторій** - Абстракція доступу до даних
2. **Паттерн Сервіс** - Інкапсуляція бізнес-логіки
3. **Ін'єкція Залежностей** - Роз'єднання між шарами
4. **Value Object** - Перерахування `CopyStatus`
5. **Інкапсуляція** - Приватні поля з валідацією в конструкторах
## Розширення для Ітерації 2
- Додано `IDataStore<T>` та `JsonFileDataStore<T>` для збільшення персистентності
- Додано `LibraryPersistenceService` для збереження/відновлення стану
- Введено `LibraryQueryService` як шар запитів і аналітики
- `IOverduePolicy` використовується як Strategy для тарифів штрафів
