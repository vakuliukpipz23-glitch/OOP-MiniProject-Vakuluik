using LibraryManagementSystem.Domain;
using LibraryManagementSystem.Domain.Repositories;

namespace LibraryManagementSystem.Infrastructure;

public class InMemoryPatronRepository : IPatronRepository
{
    private readonly Dictionary<string, Patron> _patronsDb = new();

    public void Add(Patron patron)
    {
        if (patron is null)
        {
            throw new ArgumentNullException(nameof(patron));
        }

        if (_patronsDb.ContainsKey(patron.PatronId))
        {
            throw new InvalidOperationException($"Patron {patron.PatronId} already exists");
        }

        _patronsDb[patron.PatronId] = patron;
    }

    public Patron? GetById(string patronId)
    {
        if (string.IsNullOrWhiteSpace(patronId))
        {
            throw new ArgumentException("Patron ID cannot be empty", nameof(patronId));
        }

        return _patronsDb.TryGetValue(patronId, out var patron) ? patron : null;
    }

    public List<Patron> GetAll()
    {
        return _patronsDb.Values.ToList();
    }

    public void Update(Patron patron)
    {
        if (patron is null)
        {
            throw new ArgumentNullException(nameof(patron));
        }

        if (!_patronsDb.ContainsKey(patron.PatronId))
        {
            throw new InvalidOperationException($"Patron {patron.PatronId} not found");
        }

        _patronsDb[patron.PatronId] = patron;
    }
}
