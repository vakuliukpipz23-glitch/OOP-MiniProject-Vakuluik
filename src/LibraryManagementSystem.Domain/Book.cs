namespace LibraryManagementSystem.Domain;

public class Book
{
    public string ISBN { get; }
    public string Title { get; }
    public string Author { get; }
    public string Category { get; private set; }
    public int TotalCopies { get; private set; }

    public Book(string isbn, string title, string author, string category, int totalCopies)
    {
        ValidateISBN(isbn);
        ValidateInput(title, nameof(title));
        ValidateInput(author, nameof(author));
        ValidateInput(category, nameof(category));
        ValidateTotalCopies(totalCopies);

        ISBN = isbn;
        Title = title;
        Author = author;
        Category = category;
        TotalCopies = totalCopies;
    }

    private static void ValidateISBN(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentException("ISBN cannot be empty", nameof(isbn));
        }

        if (isbn.Length < 10 || isbn.Length > 17)
        {
            throw new ArgumentException("ISBN must be between 10 and 17 characters", nameof(isbn));
        }
    }

    private static void ValidateInput(string input, string paramName)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException($"{paramName} cannot be empty", paramName);
        }

        if (input.Length > 255)
        {
            throw new ArgumentException($"{paramName} is too long (max 255 characters)", paramName);
        }
    }

    private static void ValidateTotalCopies(int totalCopies)
    {
        if (totalCopies <= 0)
        {
            throw new ArgumentException("Total copies must be greater than 0", nameof(totalCopies));
        }

        if (totalCopies > 1000)
        {
            throw new ArgumentException("Total copies cannot exceed 1000", nameof(totalCopies));
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is Book book && ISBN == book.ISBN;
    }

    public void UpdateCategory(string category)
    {
        ValidateInput(category, nameof(category));
        Category = category;
    }

    public override int GetHashCode()
    {
        return ISBN.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Title} by {Author} (ISBN: {ISBN})";
    }
}
