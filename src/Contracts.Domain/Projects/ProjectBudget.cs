namespace Contracts.Domain.Projects;

public sealed class ProjectBudget : SoftDeletableEntity
{
    private ProjectBudget()
    {
        FiscalYear = string.Empty;
    }

    public ProjectBudget(
        long projectId,
        string fiscalYear,
        long currencyId,
        int currencyScale,
        long approvedAmountMinor,
        long consumedAmountMinor,
        AuditStamp stamp)
        : base(stamp)
    {
        if (currencyScale is < 0 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(currencyScale));
        }

        ProjectId = Guard.RequiredId(projectId, nameof(projectId));
        FiscalYear = Guard.RequiredText(fiscalYear, nameof(fiscalYear), 20);
        CurrencyId = Guard.RequiredId(currencyId, nameof(currencyId));
        CurrencyScale = currencyScale;
        ApprovedAmountMinor = Guard.NonNegativeAmount(approvedAmountMinor, nameof(approvedAmountMinor));
        ConsumedAmountMinor = Guard.NonNegativeAmount(consumedAmountMinor, nameof(consumedAmountMinor));
        if (ConsumedAmountMinor > ApprovedAmountMinor)
        {
            throw new ArgumentOutOfRangeException(nameof(consumedAmountMinor), "Consumed budget cannot exceed approved budget.");
        }
    }

    public long ProjectId { get; private set; }

    public string FiscalYear { get; private set; }

    public long CurrencyId { get; private set; }

    public int CurrencyScale { get; private set; }

    public long ApprovedAmountMinor { get; private set; }

    public long ConsumedAmountMinor { get; private set; }

    public long RemainingAmountMinor => checked(ApprovedAmountMinor - ConsumedAmountMinor);

    public void Update(string fiscalYear, long currencyId, int currencyScale, long approvedAmountMinor,
        long consumedAmountMinor, AuditStamp stamp)
    {
        if (currencyScale is < 0 or > 6) throw new ArgumentOutOfRangeException(nameof(currencyScale));
        string validFiscalYear = Guard.RequiredText(fiscalYear, nameof(fiscalYear), 20);
        long validCurrencyId = Guard.RequiredId(currencyId, nameof(currencyId));
        long validApproved = Guard.NonNegativeAmount(approvedAmountMinor, nameof(approvedAmountMinor));
        long validConsumed = Guard.NonNegativeAmount(consumedAmountMinor, nameof(consumedAmountMinor));
        if (validConsumed > validApproved)
            throw new ArgumentOutOfRangeException(nameof(consumedAmountMinor), "Consumed budget cannot exceed approved budget.");
        FiscalYear = validFiscalYear; CurrencyId = validCurrencyId; CurrencyScale = currencyScale;
        ApprovedAmountMinor = validApproved; ConsumedAmountMinor = validConsumed;
        _ = RemainingAmountMinor;
        Touch(stamp);
    }
}
