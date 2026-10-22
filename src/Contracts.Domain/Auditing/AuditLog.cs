namespace Contracts.Domain.Auditing;

public sealed class AuditLog : Entity
{
    private AuditLog()
    {
        Action = string.Empty;
        EntityType = string.Empty;
        EntityId = string.Empty;
        CorrelationId = string.Empty;
        Outcome = string.Empty;
    }

    public AuditLog(
        long? actorUserId,
        string action,
        string entityType,
        string entityId,
        DateTimeOffset occurredAtUtc,
        string correlationId,
        string outcome,
        string? beforeJson,
        string? afterJson)
    {
        if (actorUserId <= 0) throw new ArgumentOutOfRangeException(nameof(actorUserId));
        if (occurredAtUtc.Offset != TimeSpan.Zero) throw new ArgumentException("Audit timestamps must use UTC.", nameof(occurredAtUtc));
        ActorUserId = actorUserId;
        Action = Guard.StableCode(action, nameof(action));
        EntityType = Guard.RequiredText(entityType, nameof(entityType), 150);
        EntityId = Guard.RequiredText(entityId, nameof(entityId), 100);
        OccurredAtUtc = occurredAtUtc;
        CorrelationId = Guard.RequiredText(correlationId, nameof(correlationId), 100);
        Outcome = Guard.StableCode(outcome, nameof(outcome));
        BeforeJson = Guard.OptionalText(beforeJson, nameof(beforeJson), 4000);
        AfterJson = Guard.OptionalText(afterJson, nameof(afterJson), 4000);
    }

    public long? ActorUserId { get; private set; }
    public string Action { get; private set; }
    public string EntityType { get; private set; }
    public string EntityId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public string CorrelationId { get; private set; }
    public string Outcome { get; private set; }
    public string? BeforeJson { get; private set; }
    public string? AfterJson { get; private set; }
}
