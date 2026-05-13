# Діаграма Послідовності - Сценарій Позичення та Повернення Книги

## Основний Потік: Позичення та Повернення Книги

```mermaid
sequenceDiagram
    actor Бібліотекар
    participant Консоль
    participant PatronService
    participant BookService
    participant BorrowService
    participant PatronRepo
    participant BookRepo
    participant BorrowRepo
    participant Domain[Доменний Шар]

    Бібліотекар->>Консоль: Запуск Застосунку
    Бібліотекар->>Консоль: Вибір "Зареєструвати Відвідувача"
    Консоль->>PatronService: RegisterPatron(name, email, phone)
    PatronService->>Domain: new Patron(name, email, phone)
    Domain->>Domain: Перевірка вхідних даних
    Domain->>PatronRepo: patron created
    PatronService-->>Консоль: Відвідувач зареєстрований
    
    Бібліотекар->>Консоль: Вибір "Позичити Книгу"
    Бібліотекар->>Консоль: Введення ISBN: "978-0134685991"
    Консоль->>BookService: FindBook(isbn)
    BookService->>BookRepo: GetById(isbn)
    BookRepo-->>BookService: Об'єкт книги з копіями
    BookService-->>Консоль: Книга знайдена (5 доступних)
    
    Консоль->>BorrowService: BorrowBook(patronId, isbn)
    BorrowService->>PatronRepo: GetById(patronId)
    PatronRepo-->>BorrowService: Об'єкт відвідувача
    BorrowService->>BookRepo: GetById(isbn)
    BookRepo-->>BorrowService: Книга зі списком копій
    BorrowService->>Domain: new BorrowRecord(patron, copy)
    Domain->>Domain: Перевірка, що відвідувач не заблокований
    Domain->>Domain: Встановлення dueDate = сьогодні + 30 днів
    Domain->>BookRepo: UpdateCopyStatus(copyId, Borrowed)
    Domain->>PatronRepo: AddBorrowRecord(patron, borrowRecord)
    BorrowService->>BorrowRepo: Add(borrowRecord)
    BorrowService-->>Консоль: Позичення успішне
    Консоль-->>Бібліотекар: Відображення: Книга позичена до [дата]
    
    Бібліотекар->>Консоль: (Через деякий час)
    Бібліотекар->>Консоль: Вибір "Повернути Книгу"
    Консоль->>BorrowService: ReturnBook(borrowId)
    BorrowService->>BorrowRepo: GetById(borrowId)
    BorrowRepo-->>BorrowService: Об'єкт BorrowRecord
    BorrowService->>Domain: CalculateOverdueFee(returnDate)
    Domain->>Domain: Перевірка, чи returnDate > dueDate
    alt Прострочено
        Domain->>Domain: Розрахунок fee = daysLate * 0.50
    else Вчасно
        Domain->>Domain: fee = 0
    end
    Domain->>BorrowRepo: UpdateBorrowRecord(borrowRecord)
    Domain->>BookRepo: UpdateCopyStatus(copyId, Available)
    BorrowService-->>Консоль: Повернення оброблено
    Консоль-->>Бібліотекар: Відображення підтвердження + штрафи (якщо є)
```

## Сценарії Винятків

### Сценарій: Книга Недоступна
```mermaid
sequenceDiagram
    participant User as Бібліотекар
    participant UI as Консоль
    participant Svc as BorrowService
    participant Repo as Repository

    User->>UI: Запит на позичення книги
    UI->>Svc: BorrowBook(patronId, isbn)
    Svc->>Repo: Знайти доступну копію
    Repo-->>Svc: Немає доступних копій
    Svc-->>UI: Виняток: Немає копій доступних
    UI-->>User: Повідомлення про помилку показано
```

### Сценарій: Невалідний Відвідувач
```mermaid
sequenceDiagram
    participant User as Бібліотекар
    participant UI as Консоль
    participant Svc as BorrowService
    participant Repo as Repository

    User->>UI: Запит на позичення книги
    UI->>Svc: BorrowBook(invalidPatronId, isbn)
    Svc->>Repo: GetPatron(invalidPatronId)
    Repo-->>Svc: Відвідувач не знайдений
    Svc-->>UI: Виняток: Відвідувач не зареєстрований
    UI-->>User: Повідомлення про помилку показано
```

## Покриття Вертикального Зрізу

Діаграма послідовності демонструє **повний вертикальний зріз**, що:

1. **Починається з UI** - Користувач взаємодіє з консоллю
2. **Проходить через Шар Застосунку** - PatronService, BookService, BorrowService оркеструють бізнес-логіку
3. **Досягає Шару Домену** - Бізнес-правила забезпечуються (валідація, розрахунок штрафів)
4. **Використовує Інфраструктуру** - Репозиторії зберігають стан in-memory
5. **Повертається до UI** - Результати відображаються користувачу

Це забезпечує:
- Чітке розділення відповідальності
- Тестованість на кожному рівні
- Майбутню розширюваність (заміна in-memory на SQL)
- Обробку помилок на межах домену
