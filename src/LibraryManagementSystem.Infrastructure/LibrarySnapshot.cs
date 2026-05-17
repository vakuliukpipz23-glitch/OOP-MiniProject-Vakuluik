using LibraryManagementSystem.Domain;

namespace LibraryManagementSystem.Infrastructure;

public record BookDto(string ISBN, string Title, string Author, string Category, int TotalCopies);
public record CopyDto(string CopyId, string BookIsbn, CopyStatus Status);
public record PatronDto(string PatronId, string Name, string Email, string Phone, DateTime RegistrationDate);
public record BorrowRecordDto(string BorrowId, string PatronId, string CopyId, DateTime BorrowDate, DateTime DueDate, DateTime? ReturnDate, decimal OverdueFee);

public record LibrarySnapshot(
    List<BookDto> Books,
    List<CopyDto> Copies,
    List<PatronDto> Patrons,
    List<BorrowRecordDto> BorrowRecords
);
