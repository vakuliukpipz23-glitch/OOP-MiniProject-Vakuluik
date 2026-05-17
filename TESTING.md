# TESTING.md

Запуск тестів локально:

1. Встановіть пакети (в разі потреби):

```powershell
cd src
dotnet restore
```

2. Запуск всіх тестів з збіркою покриття (coverlet):

```powershell
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

3. Згенерувати HTML звіт (за наявності reportgenerator):

```powershell
reportgenerator -reports:**/coverage.opencover.xml -targetdir:coverage-report
```

Що покрито:
- Юніт-тести доменної логіки (інваріанти, граничні значення)
- Інтеграційні файлові тести для persistence
- Негативні сценарії I/O

Як додати новий тест
- Тести розміщуються в `tests/LibraryManagementSystem.Tests`.
- Використовуйте xUnit та Moq для моків.

CI
- CI запускає `dotnet build` та `dotnet test` з параметром покриття.
- Pipeline має падати при невдалих тестах.