namespace Contracts.Domain.Common;

public abstract class Entity
{
    public long Id { get; protected set; }

    public long ConcurrencyVersion { get; private set; } = 1;
}

public abstract class AuditedEntity : Entity
{
    protected AuditedEntity()
    {
    }

    protected AuditedEntity(AuditStamp stamp)
    {
        CreatedAtUtc = stamp.AtUtc;
        CreatedByUserId = stamp.UserId;
        UpdatedAtUtc = stamp.AtUtc;
        UpdatedByUserId = stamp.UserId;
    }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public long? CreatedByUserId { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public long? UpdatedByUserId { get; private set; }

    public void Touch(AuditStamp stamp)
    {
        UpdatedAtUtc = stamp.AtUtc;
        UpdatedByUserId = stamp.UserId;
    }
}

public abstract class SoftDeletableEntity : AuditedEntity
{
    protected SoftDeletableEntity()
    {
    }

    protected SoftDeletableEntity(AuditStamp stamp)
        : base(stamp)
    {
    }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset? DeletedAtUtc { get; private set; }

    public long? DeletedByUserId { get; private set; }

    public void SoftDelete(AuditStamp stamp)
    {
        IsDeleted = true;
        DeletedAtUtc = stamp.AtUtc;
        DeletedByUserId = stamp.UserId;
        Touch(stamp);
    }
}
