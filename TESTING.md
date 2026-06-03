# TESTING

## Запуск тестів
```powershell
cd c:\Users\Паша\Documents\GitHub\OOP-MiniProject-Vakuluik
dotnet test
```

## Збір покриття
```powershell
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=TestResults/coverage/
```

## Звіт HTML
```powershell
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:**/coverage.opencover.xml -targetdir:coverage-report
```

## Що покрито
- Unit tests: сервіси, доменні інваріанти, negative scenarios
- Integration tests: persistence, corrupted JSON, restore state flow

## Примітки
- Інтеграційні тести використовують тимчасові файли і автоматично їх видаляють.
- Для локальної відладки можна запускати окремий тестовий метод або весь проєкт.
