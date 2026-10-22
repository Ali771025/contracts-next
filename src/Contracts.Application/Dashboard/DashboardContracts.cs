namespace Contracts.Application.Dashboard;

using global::Contracts.Application.Security;

public enum KpiValueStatus { Applicable, NotApplicable }
public enum DashboardDrilldownKind
{
    ActiveContracts, FinalValue, Payments, Receipts, Receivables, Payables,
    ExpiringContracts, ExpiringGuarantees, OpenAlerts, OverdueReceivables,
    OverduePayables, CriticalContracts, CollectionRate, PaymentRate, NetCash,
    BudgetConsumption, AverageContractorScore
}

public sealed record DashboardFilter(DateOnly? DateFrom = null, DateOnly? DateTo = null,
    long? ContractTypeId = null, long? ContractStatusId = null, long? PartyId = null,
    long? ProjectId = null, int MaxContracts = 20_000)
{
    public void Validate()
    {
        if (DateFrom.HasValue && DateTo.HasValue && DateTo < DateFrom)
            throw new ArgumentException("Invalid dashboard date range.");
        if (MaxContracts is < 1 or > 20_000) throw new ArgumentOutOfRangeException(nameof(MaxContracts));
    }
}

public sealed record DashboardMetric(string Code, string TitleFa, long Value,
    string Unit, DashboardDrilldownKind Drilldown);
public sealed record DashboardRatio(string Code, string TitleFa, decimal? Value,
    KpiValueStatus Status, string Unit, DashboardDrilldownKind Drilldown);
public sealed record ContractTypeKpi(string Code, string TitleFa, int ContractCount,
    int ActiveCount, long FinalValueRial, long ReceivableRial, long PayableRial, int CriticalCount);
public sealed record DashboardContractDetail(long ContractId, string ContractNo, string Title,
    string ContractTypeCode, string ContractTypeTitle, string PartyName, string? ProjectTitle,
    string StatusTitle, DateOnly EffectiveEndDate, long FinalValueRial, long ReceivableRial,
    long PayableRial, long TotalClaimsRial,long SettledClaimsRial,long TotalDebtsRial,long SettledDebtsRial,
    long TotalPaymentsRial, long TotalReceiptsRial,
    long OverdueReceivableRial, long OverduePayableRial, bool IsCritical);
public sealed record DashboardAlertDetail(long AlertId, long ContractId, string ContractNo,
    string Title, string PriorityCode, string PriorityTitle, DateOnly DueDate);
public sealed record DashboardEvidenceDetail(string SourceType, long SourceId, string Label,
    string? ContractNo, long Numerator, long Denominator, decimal? Value);
public sealed record DashboardDrilldown(DashboardDrilldownKind Kind, int Count, long TotalRial,
    IReadOnlyList<DashboardContractDetail> Contracts, IReadOnlyList<DashboardAlertDetail> Alerts,
    IReadOnlyList<DashboardEvidenceDetail> Evidence);
public sealed record DashboardSnapshot(Guid SnapshotId, DateTimeOffset GeneratedAtUtc,
    DateOnly BusinessDate, DashboardFilter Filter, string FilterSummary, int QueryCount,
    long ElapsedMilliseconds, IReadOnlyList<DashboardMetric> MainMetrics,
    IReadOnlyList<DashboardRatio> AdvancedMetrics, IReadOnlyList<ContractTypeKpi> ContractTypes,
    IReadOnlyList<DashboardContractDetail> TopReceivables,
    IReadOnlyList<DashboardContractDetail> TopPayables,
    IReadOnlyList<DashboardAlertDetail> OpenAlerts,
    IReadOnlyDictionary<DashboardDrilldownKind, DashboardDrilldown> Drilldowns);

public sealed record DashboardContractFact(long Id, string ContractNo, string Title,
    string TypeCode, string TypeTitle, string StatusCode, string StatusTitle, string PartyName,
    string? ProjectTitle, DateOnly EffectiveEndDate, long FinalValueRial, long TotalClaimsRial,
    long SettledClaimsRial, long TotalDebtsRial, long SettledDebtsRial, long TotalPaymentsRial,
    long TotalReceiptsRial, long OverdueReceivableRial, long OverduePayableRial,
    int CriticalOpenAlertCount);
public sealed record DashboardGuaranteeFact(long Id, long ContractId, string ContractNo, string Number,
    DateOnly ExpiryDate, long AmountRial, string StatusCode);
public sealed record DashboardAlertFact(long Id, long ContractId, string ContractNo, string Title,
    string PriorityCode, string PriorityTitle, string StatusCode, DateOnly DueDate);
public sealed record DashboardBudgetFact(long Id, string ProjectCode, string ProjectTitle, string FiscalYear,
    string CurrencyCode, long ApprovedAmountMinor, long ConsumedAmountMinor);
public sealed record DashboardEvaluationFact(long Id, string ContractNo, DateOnly Date,
    int Quality, int Time, int Financial, int Hse);
public sealed record DashboardSourceSnapshot(DateTimeOffset CapturedAtUtc, int QueryCount,
    long ElapsedMilliseconds, IReadOnlyList<DashboardContractFact> Contracts,
    IReadOnlyList<DashboardGuaranteeFact> Guarantees, IReadOnlyList<DashboardAlertFact> Alerts,
    IReadOnlyList<DashboardBudgetFact> Budgets, IReadOnlyList<DashboardEvaluationFact> Evaluations);

public interface IDashboardStore
{
    public Task<DashboardSourceSnapshot> LoadAsync(DashboardFilter filter, DateOnly today,
        int contractWarningDays, int guaranteeWarningDays, CancellationToken ct);
}

public sealed class DashboardDataLimitExceededException(int maximum) :
    InvalidOperationException($"Dashboard contract population exceeds the safe limit of {maximum}.");

public interface IDashboardQueryService
{
    public Task<DashboardSnapshot> LoadAsync(DashboardFilter filter, SecuritySection section, CancellationToken ct);
}
