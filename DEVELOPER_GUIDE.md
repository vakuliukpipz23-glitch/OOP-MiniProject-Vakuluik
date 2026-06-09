# DEVELOPER_GUIDE

## Архітектура
Проєкт розбитий на чотири логічні шари:
- `LibraryManagementSystem.Domain` — доменні сутності, інтерфейси репозиторіїв, бізнес-логіка.
- `LibraryManagementSystem.Application` — сервіси для сценаріїв використання.
- `LibraryManagementSystem.Infrastructure` — реалізації репозиторіїв, persistence, JSON-запис.
- `LibraryManagementSystem.Console` — консольний UI.

## Структура
- `Book`, `Copy`, `Patron`, `BorrowRecord`, `OverduePolicy` — основні доменні класи.
- `IBookRepository`, `IPatronRepository`, `IBorrowRepository` — інтерфейси для зберігання.
- `BookService`, `PatronService`, `BorrowService`, `LibraryQueryService` — бізнес-сервіси.
- `JsonFileDataStore<T>` та `LibraryPersistenceService` — persistence layer.

## Розширення
- Для додавання нового сховища необхідно реалізувати `IDataStore<T>`.
- Нові типи книг або політик штрафів можна додати через `IOverduePolicy`.
- Щоб додати REST API, можна створити новий проект зверху над Application та викликати сервіси.

## Запуск тестів
```bash
cd OOP-MiniProject-Vakuluik
dotnet test tests/LibraryManagementSystem.Tests/LibraryManagementSystem.Tests.csproj
```

## Запуск з coverage
```bash
dotnet test tests/LibraryManagementSystem.Tests/LibraryManagementSystem.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=TestResults/coverage/
```

## Як працює persistence
- `JsonFileDataStore<T>` читає/записує JSON у файл.
- `LibraryPersistenceService` формує `LibrarySnapshot` та відновлює стан репозиторіїв.
- Програма завантажує стан під час старту і зберігає його при виході або за запитом.

## Додаткові правила
- Публічні API сервісів повинні бути простими і валідувати вхідні дані.
- Не зберігати бізнес-логіку у `Program.cs`.
- Розширювати функціональність через сервіси, а не через консольний UI.

## Пропозиції для рефакторингу
- Винести меню та UI в окремі класи для зменшення `Program.cs`.
- Додати `IDateTimeProvider` для тестування часових сценаріїв.
- Розділити `JsonFileDataStore<T>` на generic serializer та file manager.
