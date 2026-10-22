namespace Contracts.Domain.Finance;

public sealed class PaymentAllocation : AuditedEntity
{
    private PaymentAllocation()
    {
    }

    internal PaymentAllocation(FinancialTransaction transaction, Payment payment, long amountRial, AuditStamp stamp)
        : base(stamp)
    {
        FinancialTransaction = transaction;
        Payment = payment;
        FinancialTransactionId = transaction.Id;
        PaymentId = payment.Id;
        AmountRial = Guard.PositiveAmount(amountRial, nameof(amountRial));
    }

    public long FinancialTransactionId { get; private set; }
    public long PaymentId { get; private set; }
    public long AmountRial { get; private set; }
    public FinancialTransaction? FinancialTransaction { get; private set; }
    public Payment? Payment { get; private set; }
}
