namespace Contracts.Domain.Contracts;

public sealed class Alert : SoftDeletableEntity
{
    private Alert()
    {
        Title = string.Empty;
        DeduplicationKey = string.Empty;
    }

    public Alert(
        long contractId,
        long? guaranteeId,
        long? financialTransactionId,
        long alertTypeId,
        long alertStatusId,
        long priorityId,
        string title,
        string deduplicationKey,
        DateOnly dueDate,
        AuditStamp stamp)
        : base(stamp)
    {
        ContractId = Guard.RequiredId(contractId, nameof(contractId));
        GuaranteeId = guaranteeId;
        FinancialTransactionId = financialTransactionId;
        AlertTypeId = Guard.RequiredId(alertTypeId, nameof(alertTypeId));
        AlertStatusId = Guard.RequiredId(alertStatusId, nameof(alertStatusId));
        PriorityId = Guard.RequiredId(priorityId, nameof(priorityId));
        Title = Guard.RequiredText(title, nameof(title), 300);
        DeduplicationKey = Guard.RequiredText(deduplicationKey, nameof(deduplicationKey), 200);
        DueDate = dueDate;
    }

    public long ContractId { get; private set; }
    public long? GuaranteeId { get; private set; }
    public long? FinancialTransactionId { get; private set; }
    public long AlertTypeId { get; private set; }
    public long AlertStatusId { get; private set; }
    public long PriorityId { get; private set; }
    public string Title { get; private set; }
    public string DeduplicationKey { get; private set; }
    public DateOnly DueDate { get; private set; }
    public DateTimeOffset? ResolvedAtUtc { get; private set; }
    public long? ResolvedByUserId { get; private set; }

    public void Resolve(AuditStamp stamp){AlertStatusId=2;ResolvedAtUtc=stamp.AtUtc;ResolvedByUserId=stamp.UserId;Touch(stamp);}
    public void Reopen(DateOnly dueDate,string title,AuditStamp stamp){AlertStatusId=1;DueDate=dueDate;Title=Guard.RequiredText(title,nameof(title),300);ResolvedAtUtc=null;ResolvedByUserId=null;Touch(stamp);}
    public void RefreshOpen(DateOnly dueDate,string title,long priorityId,AuditStamp stamp){if(AlertStatusId!=1)throw new InvalidOperationException("Only open alerts can be refreshed.");DueDate=dueDate;Title=Guard.RequiredText(title,nameof(title),300);PriorityId=Guard.RequiredId(priorityId,nameof(priorityId));Touch(stamp);}
}
