namespace Contracts.Domain.Contracts;

public sealed class Guarantee : SoftDeletableEntity
{
    private Guarantee()
    {
        GuaranteeNo = string.Empty;
    }

    public Guarantee(
        long contractId,
        string guaranteeNo,
        long guaranteeTypeId,
        long guaranteeStatusId,
        long amountRial,
        long? bankId,
        DateOnly issueDate,
        DateOnly expiryDate,
        string? description,
        AuditStamp stamp)
        : base(stamp)
    {
        if (expiryDate < issueDate)
        {
            throw new ArgumentException("Guarantee expiry cannot precede issue date.", nameof(expiryDate));
        }

        ContractId = Guard.RequiredId(contractId, nameof(contractId));
        GuaranteeNo = Guard.RequiredText(guaranteeNo, nameof(guaranteeNo), 100);
        GuaranteeTypeId = Guard.RequiredId(guaranteeTypeId, nameof(guaranteeTypeId));
        GuaranteeStatusId = Guard.RequiredId(guaranteeStatusId, nameof(guaranteeStatusId));
        AmountRial = Guard.PositiveAmount(amountRial, nameof(amountRial));
        BankId = bankId;
        IssueDate = issueDate;
        ExpiryDate = expiryDate;
        Description = Guard.OptionalText(description, nameof(description), 2000);
    }

    public long ContractId { get; private set; }
    public string GuaranteeNo { get; private set; }
    public long GuaranteeTypeId { get; private set; }
    public long GuaranteeStatusId { get; private set; }
    public long AmountRial { get; private set; }
    public long? BankId { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public DateOnly ExpiryDate { get; private set; }
    public string? Description { get; private set; }

    public void Update(string number,long typeId,long statusId,long amountRial,long? bankId,DateOnly issueDate,
        DateOnly expiryDate,string? description,DateOnly today,AuditStamp stamp)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(expiryDate,issueDate,nameof(expiryDate));
        GuaranteeLifecyclePolicy.ValidateTransition(GuaranteeStatusId,statusId,expiryDate,today);
        GuaranteeNo=Guard.RequiredText(number,nameof(number),100);GuaranteeTypeId=Guard.RequiredId(typeId,nameof(typeId));
        GuaranteeStatusId=Guard.RequiredId(statusId,nameof(statusId));AmountRial=Guard.PositiveAmount(amountRial,nameof(amountRial));
        BankId=bankId;IssueDate=issueDate;ExpiryDate=expiryDate;Description=Guard.OptionalText(description,nameof(description),2000);Touch(stamp);
    }
}
