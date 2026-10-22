namespace Contracts.Infrastructure.Finance;

using Contracts.Application.Finance;

public sealed class BusinessDateService(ISystemClock clock) : IBusinessDateService
{
    private static readonly TimeZoneInfo Tehran = ResolveTehran();
    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.UtcNow, Tehran).DateTime);
    private static TimeZoneInfo ResolveTehran()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran"); }
    }
}
