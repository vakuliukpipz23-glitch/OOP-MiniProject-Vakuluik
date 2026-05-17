# Test Matrix

| Use Case | Unit Tests | Integration Tests |
|---|---:|---:|
| Register Patron | ✅ (invariants, invalid input) | ✅ (create -> save -> reload) |
| Update Patron Contact | ✅ | ✅ |
| Register Book | ✅ | ✅ |
| Update Book Category | ✅ | ✅ |
| Borrow Book | ✅ (business rules) | ✅ (borrow -> save -> reload -> operate) |
| Return Book (on time / overdue) | ✅ | ✅ |
| Persistence Save/Load | ✅ (error handling) | ✅ (file roundtrip) |

Файлові тести виконуються у тимчасових файлах та не залежать від вручну створених ресурсів.