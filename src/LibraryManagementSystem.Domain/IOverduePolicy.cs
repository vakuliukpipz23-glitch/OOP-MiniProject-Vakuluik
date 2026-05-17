namespace LibraryManagementSystem.Domain;

public interface IOverduePolicy
{
    decimal FeePerDay { get; }
    int GracePeriodDays { get; }
    int MaxBorrowDays { get; }
    decimal CalculateFee(int daysLate);
}
