namespace LibraryManagementSystem.Domain;

public class OverduePolicy : IOverduePolicy
{
    public decimal FeePerDay { get; }
    public int GracePeriodDays { get; }
    public int MaxBorrowDays { get; }

    public OverduePolicy(decimal feePerDay = 0.50m, int gracePeriodDays = 0, int maxBorrowDays = 30)
    {
        ValidateFeePerDay(feePerDay);
        ValidateGracePeriod(gracePeriodDays);
        ValidateMaxBorrowDays(maxBorrowDays);

        FeePerDay = feePerDay;
        GracePeriodDays = gracePeriodDays;
        MaxBorrowDays = maxBorrowDays;
    }

    public decimal CalculateFee(int daysLate)
    {
        if (daysLate <= GracePeriodDays)
        {
            return 0;
        }

        int chargeableDays = daysLate - GracePeriodDays;
        return chargeableDays * FeePerDay;
    }

    private static void ValidateFeePerDay(decimal feePerDay)
    {
        if (feePerDay < 0)
        {
            throw new ArgumentException("Fee per day cannot be negative", nameof(feePerDay));
        }

        if (feePerDay > 100)
        {
            throw new ArgumentException("Fee per day is unreasonably high (max 100)", nameof(feePerDay));
        }
    }

    private static void ValidateGracePeriod(int gracePeriodDays)
    {
        if (gracePeriodDays < 0)
        {
            throw new ArgumentException("Grace period cannot be negative", nameof(gracePeriodDays));
        }

        if (gracePeriodDays > 30)
        {
            throw new ArgumentException("Grace period cannot exceed 30 days", nameof(gracePeriodDays));
        }
    }

    private static void ValidateMaxBorrowDays(int maxBorrowDays)
    {
        if (maxBorrowDays <= 0)
        {
            throw new ArgumentException("Max borrow days must be greater than 0", nameof(maxBorrowDays));
        }

        if (maxBorrowDays > 365)
        {
            throw new ArgumentException("Max borrow days cannot exceed 365", nameof(maxBorrowDays));
        }
    }

    public override string ToString()
    {
        return $"Fee: {FeePerDay:C}/day, Grace: {GracePeriodDays} days, Max Borrow: {MaxBorrowDays} days";
    }
}
