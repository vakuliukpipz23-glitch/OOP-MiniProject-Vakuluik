namespace LibraryManagementSystem.Domain;

public class BorrowRecord
{
    public string BorrowId { get; }
    public Patron Patron { get; }
    public Copy Copy { get; }
    public DateTime BorrowDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? ReturnDate { get; private set; }
    public decimal OverdueFee { get; private set; }

    public BorrowRecord(string borrowId, Patron patron, Copy copy, IOverduePolicy policy)
    {
        ValidateBorrowId(borrowId);
        ValidatePatron(patron);
        ValidateCopy(copy);
        ValidatePolicy(policy);

        BorrowId = borrowId;
        Patron = patron;
        Copy = copy;
        BorrowDate = DateTime.UtcNow;
        DueDate = BorrowDate.AddDays(policy.MaxBorrowDays);
        ReturnDate = null;
        OverdueFee = 0;

        Copy.MarkAsBorrowed();
        Patron.AddBorrowRecord(this);
    }

    public void ReturnBook()
    {
        if (ReturnDate.HasValue)
        {
            throw new InvalidOperationException("This book has already been returned");
        }

        ReturnDate = DateTime.UtcNow;
        Copy.MarkAsReturned();
    }

    public bool IsOverdue()
    {
        if (ReturnDate.HasValue)
        {
            return ReturnDate.Value > DueDate;
        }

        return DateTime.UtcNow > DueDate;
    }

    public int GetDaysOverdue()
    {
        DateTime checkDate = ReturnDate ?? DateTime.UtcNow;
        if (checkDate <= DueDate)
        {
            return 0;
        }

        return (int)(checkDate - DueDate).TotalDays;
    }

    public decimal CalculateOverdueFee(IOverduePolicy policy)
    {
        int daysLate = GetDaysOverdue();
        OverdueFee = policy.CalculateFee(daysLate);
        return OverdueFee;
    }

    private BorrowRecord(string borrowId, Patron patron, Copy copy, DateTime borrowDate, DateTime dueDate, DateTime? returnDate, decimal overdueFee)
    {
        ValidateBorrowId(borrowId);
        ValidatePatron(patron);
        ValidateCopy(copy);

        BorrowId = borrowId;
        Patron = patron;
        Copy = copy;
        BorrowDate = borrowDate;
        DueDate = dueDate;
        ReturnDate = returnDate;
        OverdueFee = overdueFee;

        if (!ReturnDate.HasValue && Copy.Status == CopyStatus.Available)
        {
            copy.MarkAsBorrowed();
        }

        Patron.AddBorrowRecord(this);
    }

    public static BorrowRecord Rehydrate(string borrowId, Patron patron, Copy copy, DateTime borrowDate, DateTime dueDate, DateTime? returnDate, decimal overdueFee)
    {
        if (string.IsNullOrWhiteSpace(borrowId))
        {
            throw new ArgumentException("Borrow ID cannot be empty", nameof(borrowId));
        }

        if (borrowDate == default)
        {
            throw new ArgumentException("Borrow date cannot be empty", nameof(borrowDate));
        }

        if (dueDate == default)
        {
            throw new ArgumentException("Due date cannot be empty", nameof(dueDate));
        }

        return new BorrowRecord(borrowId, patron, copy, borrowDate, dueDate, returnDate, overdueFee);
    }

    private static void ValidateBorrowId(string borrowId)
    {
        if (string.IsNullOrWhiteSpace(borrowId))
        {
            throw new ArgumentException("Borrow ID cannot be empty", nameof(borrowId));
        }

        if (borrowId.Length > 50)
        {
            throw new ArgumentException("Borrow ID is too long (max 50 characters)", nameof(borrowId));
        }
    }

    private static void ValidatePatron(Patron? patron)
    {
        if (patron is null)
        {
            throw new ArgumentNullException(nameof(patron), "Patron cannot be null");
        }
    }

    private static void ValidateCopy(Copy? copy)
    {
        if (copy is null)
        {
            throw new ArgumentNullException(nameof(copy), "Copy cannot be null");
        }
    }

    private static void ValidatePolicy(IOverduePolicy? policy)
    {
        if (policy is null)
        {
            throw new ArgumentNullException(nameof(policy), "Overdue policy cannot be null");
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is BorrowRecord record && BorrowId == record.BorrowId;
    }

    public override int GetHashCode()
    {
        return BorrowId.GetHashCode();
    }

    public override string ToString()
    {
        string status = ReturnDate.HasValue ? "Returned" : (IsOverdue() ? "OVERDUE" : "Active");
        return $"Borrow {BorrowId}: {Patron.Name} borrowed {Copy.Book.Title} - {status}";
    }
}
