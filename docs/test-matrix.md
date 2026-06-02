# Тестова Матриця

| Функціонал | Тип тесту | Файл | Очікувана поведінка |
| --- | --- | --- | --- |
| Реєстрація відвідувача | Unit | `ServiceTests.cs` | Створюється новий `Patron` з правильними даними |
| Зареєстрація книги | Unit | `ServiceTests.cs` | Створюється `Book` та відповідні `Copy` |
| Позичення книги | Unit | `ServiceTests.cs` | Створення `BorrowRecord`, оновлення статусу копії |
| Повернення книги | Unit | `ServiceTests.cs` | Статус копії повертається, обчислюється штраф |
| Повідомлення про борги | Unit | `ServiceTests.cs` | Відстеження заборгованості відвідувача |
| Збереження стану | Integration | `PersistenceIntegrationTests.cs` | Стан бібліотеки зберігається у JSON |
| Завантаження стану | Integration | `PersistenceIntegrationTests.cs` | Стан бібліотеки відновлюється з JSON |
| Відновлення стану з некоректних даних | Integration | `PersistenceIntegrationTests.cs` | Викидається `PersistenceRestoreException` |
| Пошкоджений файл JSON | Integration | `PersistenceIntegrationTests.cs` | Викидається `DataCorruptionException` |
| Пропущений файл | Integration | `PersistenceIntegrationTests.cs` | Повертається порожня колекція |
