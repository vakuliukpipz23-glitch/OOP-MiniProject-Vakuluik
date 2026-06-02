namespace LibraryManagementSystem.Domain;

public abstract class DomainException : InvalidOperationException
{
    protected DomainException(string message)
        : base(message)
    {
    }

    protected DomainException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

public sealed class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string message)
        : base(message)
    {
    }
}

public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message)
        : base(message)
    {
    }
}

public sealed class DuplicateEntityException : BusinessRuleViolationException
{
    public DuplicateEntityException(string message)
        : base(message)
    {
    }
}
