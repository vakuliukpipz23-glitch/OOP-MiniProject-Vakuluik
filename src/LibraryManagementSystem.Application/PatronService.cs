using LibraryManagementSystem.Domain;
using LibraryManagementSystem.Domain.Repositories;

namespace LibraryManagementSystem.Application;

public class PatronService
{
    private readonly IPatronRepository _patronRepository;

    public PatronService(IPatronRepository patronRepository)
    {
        _patronRepository = patronRepository ?? throw new ArgumentNullException(nameof(patronRepository));
    }

    public Patron RegisterPatron(string name, string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty", nameof(name));
        }

        string patronId = GeneratePatronId();
        var patron = new Patron(patronId, name, email, phone);
        _patronRepository.Add(patron);

        return patron;
    }

    public Patron? GetPatron(string patronId)
    {
        return _patronRepository.GetById(patronId);
    }

    public void UpdatePatronContact(string patronId, string email, string phone)
    {
        var patron = _patronRepository.GetById(patronId);
        if (patron is null)
        {
            throw new InvalidOperationException($"Patron {patronId} not found");
        }

        patron.UpdateContact(email, phone);
    }

    public List<Patron> GetAllPatrons()
    {
        return _patronRepository.GetAll().ToList();
    }

    public List<BorrowRecord> GetPatronBorrows(string patronId)
    {
        var patron = _patronRepository.GetById(patronId);
        if (patron is null)
        {
            throw new InvalidOperationException($"Patron {patronId} not found");
        }

        return patron.GetActiveBorrows();
    }

    public List<BorrowRecord> GetPatronOverdueBooks(string patronId)
    {
        var patron = _patronRepository.GetById(patronId);
        if (patron is null)
        {
            throw new InvalidOperationException($"Patron {patronId} not found");
        }

        return patron.GetOverdueBooks();
    }

    public decimal GetPatronDebts(string patronId)
    {
        var patron = _patronRepository.GetById(patronId);
        if (patron is null)
        {
            throw new InvalidOperationException($"Patron {patronId} not found");
        }

        return patron.GetTotalOverdueFeesOwed();
    }

    private static string GeneratePatronId()
    {
        return $"P{DateTime.UtcNow.Ticks}";
    }
}
