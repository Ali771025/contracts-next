namespace Contracts.Domain.Contracts;

public sealed class ContractAmendment : SoftDeletableEntity
{
    private ContractAmendment()
    {
        AmendmentNo = string.Empty;
    }

    internal ContractAmendment(
        string amendmentNo,
        DateOnly amendmentDate,
        long amountChangeRial,
        int timeExtensionDays,
        string? subject,
        string? attachmentPath,
        string? description,
        AuditStamp stamp)
        : base(stamp)
    {
        AmendmentNo = Guard.RequiredText(amendmentNo, nameof(amendmentNo), 100);
        AmendmentDate = amendmentDate;
        AmountChangeRial = amountChangeRial;
        ArgumentOutOfRangeException.ThrowIfNegative(timeExtensionDays);
        TimeExtensionDays = timeExtensionDays;
        Subject = Guard.OptionalText(subject, nameof(subject), 300);
        AttachmentPath = Guard.OptionalText(attachmentPath, nameof(attachmentPath), 1000);
        Description = Guard.OptionalText(description, nameof(description), 2000);
    }

    public long ContractId { get; private set; }
    public string AmendmentNo { get; private set; }
    public DateOnly AmendmentDate { get; private set; }
    public long AmountChangeRial { get; private set; }
    public int TimeExtensionDays { get; private set; }
    public string? Subject { get; private set; }
    public string? AttachmentPath { get; private set; }
    public string? Description { get; private set; }

    public void Update(string amendmentNo, DateOnly amendmentDate, long amountChangeRial, int timeExtensionDays,
        string? subject, string? attachmentPath, string? description, long contractBaseAmountRial,
        long otherAmendmentsTotalRial, AuditStamp stamp)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(timeExtensionDays);
        if (checked(checked(contractBaseAmountRial + otherAmendmentsTotalRial) + amountChangeRial) < 0)
            throw new InvalidOperationException("An amendment cannot make the contract final amount negative.");
        AmendmentNo=Guard.RequiredText(amendmentNo,nameof(amendmentNo),100);AmendmentDate=amendmentDate;
        AmountChangeRial=amountChangeRial;TimeExtensionDays=timeExtensionDays;
        Subject=Guard.OptionalText(subject,nameof(subject),300);AttachmentPath=Guard.OptionalText(attachmentPath,nameof(attachmentPath),1000);
        Description=Guard.OptionalText(description,nameof(description),2000);Touch(stamp);
    }
}
