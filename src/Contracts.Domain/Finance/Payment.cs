namespace Contracts.Domain.Finance;

public sealed class Payment : SoftDeletableEntity
{
    private readonly List<PaymentAllocation> allocations = [];

    private Payment()
    {
        PaymentNo = string.Empty;
        ReferenceNo = string.Empty;
    }

    public Payment(
        long contractId,
        string paymentNo,
        long amountRial,
        DateOnly paymentDate,
        long paymentTypeId,
        long? bankId,
        string? referenceNo,
        string? invoiceNo,
        string? chequeNo,
        string? payerDescription,
        string? attachmentPath,
        string? description,
        AuditStamp stamp)
        : base(stamp)
    {
        ContractId = Guard.RequiredId(contractId, nameof(contractId));
        PaymentNo = Guard.RequiredText(paymentNo, nameof(paymentNo), 100);
        AmountRial = Guard.PositiveAmount(amountRial, nameof(amountRial));
        PaymentDate = paymentDate;
        PaymentTypeId = Guard.RequiredId(paymentTypeId, nameof(paymentTypeId));
        BankId = bankId;
        ReferenceNo = Guard.OptionalText(referenceNo, nameof(referenceNo), 100) ?? string.Empty;
        InvoiceNo = Guard.OptionalText(invoiceNo, nameof(invoiceNo), 100);
        ChequeNo = Guard.OptionalText(chequeNo, nameof(chequeNo), 100);
        PayerDescription = Guard.OptionalText(payerDescription, nameof(payerDescription), 500);
        AttachmentPath = Guard.OptionalText(attachmentPath, nameof(attachmentPath), 1000);
        Description = Guard.OptionalText(description, nameof(description), 2000);
    }

    public long ContractId { get; private set; }
    public string PaymentNo { get; private set; }
    public long AmountRial { get; private set; }
    public DateOnly PaymentDate { get; private set; }
    public long PaymentTypeId { get; private set; }
    public long? BankId { get; private set; }
    public string ReferenceNo { get; private set; }
    public string? InvoiceNo { get; private set; }
    public string? ChequeNo { get; private set; }
    public string? PayerDescription { get; private set; }
    public string? AttachmentPath { get; private set; }
    public string? Description { get; private set; }
    public IReadOnlyCollection<PaymentAllocation> Allocations => allocations.AsReadOnly();
    public long RemainingAmountRial => allocations.Aggregate(AmountRial, (remaining, allocation) => checked(remaining - allocation.AmountRial));

    internal void AddAllocation(PaymentAllocation allocation) => allocations.Add(allocation);
    internal void RemoveAllocation(PaymentAllocation allocation) => allocations.Remove(allocation);

    public void Update(string paymentNo, long amountRial, DateOnly paymentDate, long paymentTypeId, long? bankId,
        string? referenceNo, string? invoiceNo, string? chequeNo, string? payerDescription,
        string? attachmentPath, string? description, AuditStamp stamp)
    {
        long validatedAmount = Guard.PositiveAmount(amountRial, nameof(amountRial));
        if (validatedAmount < Allocations.Sum(item => item.AmountRial))
            throw new InvalidOperationException("Payment amount cannot be lower than its allocations.");
        PaymentNo = Guard.RequiredText(paymentNo, nameof(paymentNo), 100);
        AmountRial = validatedAmount; PaymentDate = paymentDate;
        PaymentTypeId = Guard.RequiredId(paymentTypeId, nameof(paymentTypeId)); BankId = bankId;
        ReferenceNo = Guard.OptionalText(referenceNo, nameof(referenceNo), 100) ?? string.Empty;
        InvoiceNo = Guard.OptionalText(invoiceNo, nameof(invoiceNo), 100);
        ChequeNo = Guard.OptionalText(chequeNo, nameof(chequeNo), 100);
        PayerDescription = Guard.OptionalText(payerDescription, nameof(payerDescription), 500);
        AttachmentPath = Guard.OptionalText(attachmentPath, nameof(attachmentPath), 1000);
        Description = Guard.OptionalText(description, nameof(description), 2000); Touch(stamp);
    }
}
