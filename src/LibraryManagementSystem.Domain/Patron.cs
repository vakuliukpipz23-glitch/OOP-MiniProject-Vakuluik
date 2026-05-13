namespace LibraryManagementSystem.Domain;

public class Patron
{
    private readonly List<BorrowRecord> _borrowHistory = new();

    public string PatronId { get; }
    public string Name { get; }
    public string Email { get; }
    public string Phone { get; }
    public DateTime RegistrationDate { get; }
    public IReadOnlyList<BorrowRecord> BorrowHistory => _borrowHistory.AsReadOnly();

    public Patron(string patronId, string name, string email, string phone)
    {
        ValidatePatronId(patronId);
        ValidateName(name);
        ValidateEmail(email);
        ValidatePhone(phone);

        PatronId = patronId;
        Name = name;
        Email = email;
        Phone = phone;
        RegistrationDate = DateTime.UtcNow;
    }

    public void AddBorrowRecord(BorrowRecord record)
    {
        if (record is null)
        {
            throw new ArgumentNullException(nameof(record), "Borrow record cannot be null");
        }

        _borrowHistory.Add(record);
    }

    public List<BorrowRecord> GetActiveBorrows()
    {
        return _borrowHistory
            .Where(b => b.ReturnDate is null)
            .ToList();
    }

    public List<BorrowRecord> GetOverdueBooks()
    {
        return _borrowHistory
            .Where(b => b.ReturnDate is null && b.IsOverdue())
            .ToList();
    }

    public decimal GetTotalOverdueFeesOwed()
    {
        return _borrowHistory
            .Sum(b => b.OverdueFee);
    }

    private static void ValidatePatronId(string patronId)
    {
        if (string.IsNullOrWhiteSpace(patronId))
        {
            throw new ArgumentException("Patron ID cannot be empty", nameof(patronId));
        }

        if (patronId.Length > 50)
        {
            throw new ArgumentException("Patron ID is too long (max 50 characters)", nameof(patronId));
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty", nameof(name));
        }

        if (name.Length > 255)
        {
            throw new ArgumentException("Name is too long (max 255 characters)", nameof(name));
        }
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty", nameof(email));
        }

        if (email.Length > 255)
        {
            throw new ArgumentException("Email is too long (max 255 characters)", nameof(email));
        }

        if (!email.Contains("@"))
        {
            throw new ArgumentException("Invalid email format", nameof(email));
        }
    }

    private static void ValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException("Phone cannot be empty", nameof(phone));
        }

        if (phone.Length > 20)
        {
            throw new ArgumentException("Phone is too long (max 20 characters)", nameof(phone));
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is Patron patron && PatronId == patron.PatronId;
    }

    public override int GetHashCode()
    {
        return PatronId.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Name} (ID: {PatronId}) - {Email}";
    }
}
