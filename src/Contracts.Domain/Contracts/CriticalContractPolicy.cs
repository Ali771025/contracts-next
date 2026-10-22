namespace Contracts.Domain.Contracts;

public static class CriticalContractPolicy
{
    public static bool IsCritical(bool isActive, DateOnly effectiveEndDate, DateOnly today,
        long overdueReceivableRial, long overduePayableRial, int criticalOpenAlertCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(overdueReceivableRial);
        ArgumentOutOfRangeException.ThrowIfNegative(overduePayableRial);
        ArgumentOutOfRangeException.ThrowIfNegative(criticalOpenAlertCount);
        return isActive && effectiveEndDate < today || overdueReceivableRial > 0 ||
            overduePayableRial > 0 || criticalOpenAlertCount > 0;
    }
}
