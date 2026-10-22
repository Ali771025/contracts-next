namespace Contracts.Domain.Contracts;

public static class ContractTimelinePolicy
{
    public static DateOnly EffectiveEndDate(DateOnly baseEndDate,int extensionDays)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(extensionDays);
        return baseEndDate.AddDays(extensionDays);
    }

    public static DateOnly EffectiveEndDate(DateOnly baseEndDate, IEnumerable<ContractAmendment> amendments)
    {
        ArgumentNullException.ThrowIfNull(amendments);
        int extension = amendments.Aggregate(0, (sum, item) => checked(sum + item.TimeExtensionDays));
        return EffectiveEndDate(baseEndDate,extension);
    }
}
