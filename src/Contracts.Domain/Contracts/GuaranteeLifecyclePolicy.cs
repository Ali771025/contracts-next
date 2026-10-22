namespace Contracts.Domain.Contracts;

public static class GuaranteeLifecyclePolicy
{
    public static long EffectiveStatus(long persistedStatusId, DateOnly expiryDate, DateOnly today) =>
        persistedStatusId == 1 && expiryDate < today ? 3 : persistedStatusId;

    public static void ValidateTransition(long currentStatusId,long requestedStatusId,DateOnly expiryDate,DateOnly today)
    {
        if (requestedStatusId is < 1 or > 4) throw new ArgumentOutOfRangeException(nameof(requestedStatusId));
        if (currentStatusId is 2 or 4 && requestedStatusId != currentStatusId)
            throw new InvalidOperationException("Released or claimed guarantees are terminal.");
        if (expiryDate < today && requestedStatusId == 1)
            throw new InvalidOperationException("An expired guarantee cannot remain active.");
    }
}
