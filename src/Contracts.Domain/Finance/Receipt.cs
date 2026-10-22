namespace Contracts.Domain.Finance;

public sealed class Receipt : SoftDeletableEntity
{
    private readonly List<ReceiptAllocation> allocations = [];

    private Receipt()
    {
        ReceiptNo = string.Empty;
        ReferenceNo = string.Empty;
    }

    public Receipt(
        long contractId,
        string receiptNo,
        long amountRial,
        DateOnly receiptDate,
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
        ReceiptNo = Guard.RequiredText(receiptNo, nameof(receiptNo), 100);
        AmountRial = Guard.PositiveAmount(amountRial, nameof(amountRial));
        ReceiptDate = receiptDate;
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
    public string ReceiptNo { get; private set; }
    public long AmountRial { get; private set; }
    public DateOnly ReceiptDate { get; private set; }
    public long PaymentTypeId { get; private set; }
    public long? BankId { get; private set; }
    public string ReferenceNo { get; private set; }
    public string? InvoiceNo { get; private set; }
    public string? ChequeNo { get; private set; }
    public string? PayerDescription { get; private set; }
    public string? AttachmentPath { get; private set; }
    public string? Description { get; private set; }
    public IReadOnlyCollection<ReceiptAllocation> Allocations => allocations.AsReadOnly();
    public long RemainingAmountRial => allocations.Aggregate(AmountRial, (remaining, allocation) => checked(remaining - allocation.AmountRial));

    internal void AddAllocation(ReceiptAllocation allocation) => allocations.Add(allocation);
    internal void RemoveAllocation(ReceiptAllocation allocation) => allocations.Remove(allocation);

    public void Update(string receiptNo, long amountRial, DateOnly receiptDate, long paymentTypeId, long? bankId,
        string? referenceNo, string? invoiceNo, string? chequeNo, string? payerDescription,
        string? attachmentPath, string? description, AuditStamp stamp)
    {
        long validatedAmount = Guard.PositiveAmount(amountRial, nameof(amountRial));
        if (validatedAmount < Allocations.Sum(item => item.AmountRial))
            throw new InvalidOperationException("Receipt amount cannot be lower than its allocations.");
        ReceiptNo = Guard.RequiredText(receiptNo, nameof(receiptNo), 100);
        AmountRial = validatedAmount; ReceiptDate = receiptDate;
        PaymentTypeId = Guard.RequiredId(paymentTypeId, nameof(paymentTypeId)); BankId = bankId;
        ReferenceNo = Guard.OptionalText(referenceNo, nameof(referenceNo), 100) ?? string.Empty;
        InvoiceNo = Guard.OptionalText(invoiceNo, nameof(invoiceNo), 100);
        ChequeNo = Guard.OptionalText(chequeNo, nameof(chequeNo), 100);
        PayerDescription = Guard.OptionalText(payerDescription, nameof(payerDescription), 500);
        AttachmentPath = Guard.OptionalText(attachmentPath, nameof(attachmentPath), 1000);
        Description = Guard.OptionalText(description, nameof(description), 2000); Touch(stamp);
    }
}
