namespace LibraryManagementSystem.Infrastructure;

public abstract class DataStoreException : Exception
{
    protected DataStoreException(string message)
        : base(message)
    {
    }

    protected DataStoreException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

public class DataLoadException : DataStoreException
{
    public DataLoadException(string message)
        : base(message)
    {
    }

    public DataLoadException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

public sealed class DataSaveException : DataStoreException
{
    public DataSaveException(string message)
        : base(message)
    {
    }

    public DataSaveException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

public sealed class DataCorruptionException : DataLoadException
{
    public DataCorruptionException(string message)
        : base(message)
    {
    }

    public DataCorruptionException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

public sealed class PersistenceRestoreException : DataLoadException
{
    public PersistenceRestoreException(string message)
        : base(message)
    {
    }

    public PersistenceRestoreException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
