namespace Contracts.Domain.Finance;

public sealed class ReceiptAllocation : AuditedEntity
{
    private ReceiptAllocation()
    {
    }

    internal ReceiptAllocation(FinancialTransaction transaction, Receipt receipt, long amountRial, AuditStamp stamp)
        : base(stamp)
    {
        FinancialTransaction = transaction;
        Receipt = receipt;
        FinancialTransactionId = transaction.Id;
        ReceiptId = receipt.Id;
        AmountRial = Guard.PositiveAmount(amountRial, nameof(amountRial));
    }

    public long FinancialTransactionId { get; private set; }
    public long ReceiptId { get; private set; }
    public long AmountRial { get; private set; }
    public FinancialTransaction? FinancialTransaction { get; private set; }
    public Receipt? Receipt { get; private set; }
}
