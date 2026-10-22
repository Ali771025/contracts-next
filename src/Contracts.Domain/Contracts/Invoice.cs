namespace Contracts.Domain.Contracts;

public sealed class Invoice : SoftDeletableEntity
{
    private Invoice()
    {
        InvoiceNo = string.Empty;
    }

    public Invoice(
        long contractId,
        string invoiceNo,
        DateOnly invoiceDate,
        long invoiceTypeId,
        long paymentStatusId,
        long amountRial,
        long vatAmountRial,
        DateOnly? dueDate,
        string? description,
        AuditStamp stamp)
        : base(stamp)
    {
        ContractId = Guard.RequiredId(contractId, nameof(contractId));
        InvoiceNo = Guard.RequiredText(invoiceNo, nameof(invoiceNo), 100);
        InvoiceDate = invoiceDate;
        InvoiceTypeId = Guard.RequiredId(invoiceTypeId, nameof(invoiceTypeId));
        PaymentStatusId = Guard.RequiredId(paymentStatusId, nameof(paymentStatusId));
        AmountRial = Guard.NonNegativeAmount(amountRial, nameof(amountRial));
        VatAmountRial = Guard.NonNegativeAmount(vatAmountRial, nameof(vatAmountRial));
        _ = TotalAmountRial;
        DueDate = dueDate;
        if (dueDate.HasValue) ArgumentOutOfRangeException.ThrowIfLessThan(dueDate.Value, invoiceDate, nameof(dueDate));
        Description = Guard.OptionalText(description, nameof(description), 2000);
    }

    public long ContractId { get; private set; }
    public string InvoiceNo { get; private set; }
    public DateOnly InvoiceDate { get; private set; }
    public long InvoiceTypeId { get; private set; }
    public long PaymentStatusId { get; private set; }
    public long AmountRial { get; private set; }
    public long VatAmountRial { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public string? Description { get; private set; }
    public long TotalAmountRial => InvoiceCalculationPolicy.Total(AmountRial,VatAmountRial);

    public void Update(string number,DateOnly date,long typeId,long statusId,long amountRial,long vatRial,
        DateOnly? dueDate,string? description,AuditStamp stamp)
    {
        if(dueDate.HasValue)ArgumentOutOfRangeException.ThrowIfLessThan(dueDate.Value,date,nameof(dueDate));
        InvoiceNo=Guard.RequiredText(number,nameof(number),100);InvoiceDate=date;InvoiceTypeId=Guard.RequiredId(typeId,nameof(typeId));
        PaymentStatusId=Guard.RequiredId(statusId,nameof(statusId));AmountRial=Guard.NonNegativeAmount(amountRial,nameof(amountRial));
        VatAmountRial=Guard.NonNegativeAmount(vatRial,nameof(vatRial));_=TotalAmountRial;DueDate=dueDate;
        Description=Guard.OptionalText(description,nameof(description),2000);Touch(stamp);
    }
}
