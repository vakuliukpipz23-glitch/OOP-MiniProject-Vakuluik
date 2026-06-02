# Test Matrix — Lab 36

Use case -> тестово-перевірочні сценарії

- UC1: Реєстрація книги та управління копіями
  - Unit: BookService_RegisterBook_ValidData_CreatesBook
  - Unit: BookService_RegisterBook_DuplicateISBN_Throws
  - Unit: BookService_GetAvailableCopiesCount_ReturnsCorrectCount
  - Integration: save/load snapshot з копіями

- UC2: Borrow/Return flow
  - Unit: BorrowService_BorrowBook_ValidPatronAndBook_CreatesBorrowRecord
  - Unit: BorrowService_BorrowBook_NoAvailableCopies_ThrowsException
  - Unit: BorrowService_ReturnBook_AlreadyReturned_ThrowsException
  - Unit: BorrowService_GetPatronActiveBorrows_ReturnsOnlyUnreturned
  - Integration: save->restore->borrow state after restore

- UC3: Overdue / debt logic
  - Unit: QueryService_GetPatronsWithOverdueDebt_ReturnsOverduePatrons
  - Unit: BorrowService_GetPatronTotalDebts_ReturnsExpectedDebt
  - Integration: restore overdue borrow with due date in past

- UC4: Persistence integrity
  - Integration: JsonFileDataStore_SaveAndLoad_PersistsSnapshot
  - Integration: JsonFileDataStore_Load_CorruptedFile_Throws
  - Integration: LibraryPersistenceService_Restore_MissingBook_Throws
  - Integration: LibraryPersistenceService_Restore_MissingPatron_Throws
  - Integration: Persistence_SaveSequential_Succeeds

## Відповідність
- Unit tests: перевірка бізнес-інваріантів і edge cases
- Integration tests: перевірка end-to-end persistence та поведінки при хибних даних
