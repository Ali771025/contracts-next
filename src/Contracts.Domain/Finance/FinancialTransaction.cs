namespace Contracts.Domain.Finance;

public sealed class FinancialTransaction : SoftDeletableEntity
{
    private readonly List<PaymentAllocation> paymentAllocations = [];
    private readonly List<ReceiptAllocation> receiptAllocations = [];

    private FinancialTransaction()
    {
        TransactionNo = string.Empty;
        Title = string.Empty;
    }

    public FinancialTransaction(
        string transactionNo,
        string title,
        long contractId,
        long transactionTypeId,
        long statusId,
        long amountRial,
        DateOnly transactionDate,
        DateOnly dueDate,
        string? description,
        AuditStamp stamp)
        : base(stamp)
    {
        if (transactionTypeId is not ReferenceIds.TransactionTypes.Claim and not ReferenceIds.TransactionTypes.Debt)
        {
            throw new ArgumentOutOfRangeException(nameof(transactionTypeId));
        }

        TransactionNo = Guard.RequiredText(transactionNo, nameof(transactionNo), 100);
        Title = Guard.RequiredText(title, nameof(title), 300);
        ContractId = Guard.RequiredId(contractId, nameof(contractId));
        TransactionTypeId = transactionTypeId;
        StatusId = Guard.RequiredId(statusId, nameof(statusId));
        AmountRial = Guard.PositiveAmount(amountRial, nameof(amountRial));
        TransactionDate = transactionDate;
        ArgumentOutOfRangeException.ThrowIfLessThan(dueDate, transactionDate, nameof(dueDate));
        DueDate = dueDate;
        Description = Guard.OptionalText(description, nameof(description), 2000);
    }

    public string TransactionNo { get; private set; }
    public string Title { get; private set; }
    public long ContractId { get; private set; }
    public long TransactionTypeId { get; private set; }
    public long StatusId { get; private set; }
    public long AmountRial { get; private set; }
    public DateOnly TransactionDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public string? Description { get; private set; }
    public IReadOnlyCollection<PaymentAllocation> PaymentAllocations => paymentAllocations.AsReadOnly();
    public IReadOnlyCollection<ReceiptAllocation> ReceiptAllocations => receiptAllocations.AsReadOnly();
    public long AllocatedAmountRial => TransactionTypeId == ReferenceIds.TransactionTypes.Claim
        ? receiptAllocations.Sum(allocation => allocation.AmountRial)
        : paymentAllocations.Sum(allocation => allocation.AmountRial);
    public long RemainingAmountRial => checked(AmountRial - AllocatedAmountRial);

    public void Update(DateOnly transactionDate, DateOnly dueDate, long amountRial, long statusId,
        string? description, AuditStamp stamp)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dueDate, transactionDate, nameof(dueDate));
        long validatedAmount = Guard.PositiveAmount(amountRial, nameof(amountRial));
        if (validatedAmount < AllocatedAmountRial)
            throw new InvalidOperationException("Obligation amount cannot be lower than its allocations.");
        TransactionDate = transactionDate;
        DueDate = dueDate;
        AmountRial = validatedAmount;
        StatusId = Guard.RequiredId(statusId, nameof(statusId));
        Description = Guard.OptionalText(description, nameof(description), 2000);
        Touch(stamp);
    }

    public void SetStatus(long statusId, AuditStamp stamp)
    {
        StatusId = Guard.RequiredId(statusId, nameof(statusId));
        Touch(stamp);
    }

    internal void AddAllocation(PaymentAllocation allocation) => paymentAllocations.Add(allocation);
    internal void AddAllocation(ReceiptAllocation allocation) => receiptAllocations.Add(allocation);
    internal void RemoveAllocation(PaymentAllocation allocation) => paymentAllocations.Remove(allocation);
    internal void RemoveAllocation(ReceiptAllocation allocation) => receiptAllocations.Remove(allocation);
}
