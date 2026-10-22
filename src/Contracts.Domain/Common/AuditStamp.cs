namespace Contracts.Domain.Common;

public readonly record struct AuditStamp
{
    public AuditStamp(long? userId, DateTimeOffset atUtc)
    {
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(userId));
        }

        if (atUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Audit timestamps must use UTC.", nameof(atUtc));
        }

        UserId = userId;
        AtUtc = atUtc;
    }

    public long? UserId { get; }

    public DateTimeOffset AtUtc { get; }
}
