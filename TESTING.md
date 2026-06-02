# Тестування проекту

## Запуск всіх тестів

Виконайте з кореневого каталогу проєкту:

```bash
dotnet test
```

## Запуск одиничних тестів

Щоб виконати тести для конкретного класа:

```bash
dotnet test --filter "FullyQualifiedName~DomainModelTests"
```

## Запуск тестів з покриттям коду

```bash
dotnet test --configuration Release --collect:"XPlat Code Coverage" /p:CoverletOutputFormat=opencover /p:CoverletOutput=TestResults/coverage/
```

## Тестування персистенції

Інтеграційні тести зберігають і завантажують дані з тимчасового JSON-файлу.

## Обробка помилок

Тести перевіряють:
- доменні винятки (`EntityNotFoundException`, `BusinessRuleViolationException`)
- помилки збереження/завантаження (`DataLoadException`, `DataSaveException`, `DataCorruptionException`)
- відновлення стану з некоректних даних (`PersistenceRestoreException`)
