namespace LibraryManagementSystem.Domain;

public class Copy
{
    public string CopyId { get; }
    public Book Book { get; }
    public CopyStatus Status { get; private set; }

    public Copy(string copyId, Book book)
    {
        ValidateCopyId(copyId);
        ValidateBook(book);

        CopyId = copyId;
        Book = book;
        Status = CopyStatus.Available;
    }

    public void MarkAsBorrowed()
    {
        if (Status != CopyStatus.Available)
        {
            throw new InvalidOperationException(
                $"Cannot borrow copy in {Status} status. Only Available copies can be borrowed.");
        }

        Status = CopyStatus.Borrowed;
    }

    public void MarkAsReturned()
    {
        if (Status != CopyStatus.Borrowed)
        {
            throw new InvalidOperationException(
                $"Cannot return copy in {Status} status. Only Borrowed copies can be returned.");
        }

        Status = CopyStatus.Available;
    }

    public void MarkAsReserved()
    {
        if (Status != CopyStatus.Available)
        {
            throw new InvalidOperationException(
                $"Cannot reserve copy in {Status} status. Only Available copies can be reserved.");
        }

        Status = CopyStatus.Reserved;
    }

    public void MarkAsDamaged()
    {
        Status = CopyStatus.Damaged;
    }

    private static void ValidateCopyId(string copyId)
    {
        if (string.IsNullOrWhiteSpace(copyId))
        {
            throw new ArgumentException("Copy ID cannot be empty", nameof(copyId));
        }

        if (copyId.Length > 50)
        {
            throw new ArgumentException("Copy ID is too long (max 50 characters)", nameof(copyId));
        }
    }

    private static void ValidateBook(Book? book)
    {
        if (book is null)
        {
            throw new ArgumentNullException(nameof(book), "Book cannot be null");
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is Copy copy && CopyId == copy.CopyId;
    }

    public override int GetHashCode()
    {
        return CopyId.GetHashCode();
    }

    public override string ToString()
    {
        return $"Copy {CopyId} of {Book.Title} ({Status})";
    }
}
