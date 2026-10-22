namespace Contracts.Domain.Finance;

public static class AllocationPolicy
{
    public static PaymentAllocation Allocate(
        FinancialTransaction transaction,
        Payment payment,
        long amountRial,
        AuditStamp stamp)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(payment);
        ValidateCommon(transaction, payment.ContractId, amountRial);

        if (transaction.TransactionTypeId != ReferenceIds.TransactionTypes.Debt)
        {
            throw new InvalidOperationException("Only DEBT obligations can be settled by payments.");
        }

        if (amountRial > payment.RemainingAmountRial)
        {
            throw new InvalidOperationException("Allocation exceeds the payment remaining amount.");
        }

        PaymentAllocation allocation = new(transaction, payment, amountRial, stamp);
        transaction.AddAllocation(allocation);
        payment.AddAllocation(allocation);
        return allocation;
    }

    public static ReceiptAllocation Allocate(
        FinancialTransaction transaction,
        Receipt receipt,
        long amountRial,
        AuditStamp stamp)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(receipt);
        ValidateCommon(transaction, receipt.ContractId, amountRial);

        if (transaction.TransactionTypeId != ReferenceIds.TransactionTypes.Claim)
        {
            throw new InvalidOperationException("Only CLAIM obligations can be settled by receipts.");
        }

        if (amountRial > receipt.RemainingAmountRial)
        {
            throw new InvalidOperationException("Allocation exceeds the receipt remaining amount.");
        }

        ReceiptAllocation allocation = new(transaction, receipt, amountRial, stamp);
        transaction.AddAllocation(allocation);
        receipt.AddAllocation(allocation);
        return allocation;
    }

    private static void ValidateCommon(FinancialTransaction transaction, long sourceContractId, long amountRial)
    {
        Guard.PositiveAmount(amountRial, nameof(amountRial));
        if (transaction.ContractId != sourceContractId)
        {
            throw new InvalidOperationException("Allocation source and obligation must belong to the same contract.");
        }

        if (amountRial > transaction.RemainingAmountRial)
        {
            throw new InvalidOperationException("Allocation exceeds the obligation remaining amount.");
        }
    }
}
