# Syllabus Coverage

## Основи ООП
- Використано обов’язково: класи, інкапсуляція, наслідування не застосовано як основна модель, але об’єкти і методи чітко розділені.
- Частково: поліморфізм реалізовано через інтерфейси репозиторіїв.
- Розширення: додаткові generic utility у майбутньому.

## Абстракції, поліморфізм, інтерфейси
- Використано обов’язково: `IBookRepository`, `IPatronRepository`, `IBorrowRepository`, `IDataStore<T>`.
- Частково: `IOverduePolicy` як стратегія.
- Розширення: можна додати Adapter/Facade для нового джерела даних.

## Generics, колекції, LINQ, делегати
- Використано обов’язково: `List<T>`, `IEnumerable<T>`, `HashSet<T>`.
- LINQ: фільтрація, групування, агрегація та сортування у `LibraryQueryService`.
- Частково: делегати не застосовані явно.

## Обробка помилок і persistence
- Використано обов’язково: `ArgumentException`, `InvalidOperationException`, контроль стану в сервісах.
- Persistence: асинхронний JSON save/load, відновлення стану через snapshot.

## SOLID
- S: роздільні класи для домену, сервіси, інфраструктура.
- O: відкритість для додавання нових політик штрафів, закритість для модифікації сервісів.
- L: замінність інтерфейсів репозиторіїв.
- I: розділені інтерфейси для різних видів зберігання.
- D: залежність від абстракцій, а не конкретних реалізацій.

## Патерни
- Репозиторій: `IBookRepository`/`IPatronRepository`/`IBorrowRepository`.
- Стратегія: `IOverduePolicy`.
- Сервіс: бізнес-логіка у `BookService`, `BorrowService`, `PatronService`, `LibraryQueryService`.
- Фасад: `LibraryPersistenceService` як точка взаємодії persistence.

## UML
- Наявні діаграми: `docs/class-diagram.md`, `docs/sequence-diagram.md`.
- Частково: можна додати deployment-діаграми для майбутніх релізів.

## Тестування
- Юніт-тести та інтеграційні тести: `tests/LibraryManagementSystem.Tests`.
- Negative scenarios: коректна обробка відсутності копій, пошкоджених файлів, подвійних повернень.
- Coverage: збір у CI.

## Рефакторинг
- Усунуті smells: розділення методів у `Program.cs`, абстракція persistence, валідація даних.
- Частково: можна додати ще більше DI та замінити статичні виклики.

## Розширення на фінальному етапі
- `IDataStore<T>` і JSON persistence.
- `LibraryQueryService` з аналітикою та звітністю.
- Документація release-level: `FINAL_REPORT.md`, `release-plan.md`, `DEMO.md`, `docs/defense-qa.md`.
