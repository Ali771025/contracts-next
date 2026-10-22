namespace Contracts.Application.Dashboard;

using global::Contracts.Application.Finance;
using global::Contracts.Application.Operations;
using global::Contracts.Application.Security;
using global::Contracts.Domain.Contracts;
using global::Contracts.Domain.Finance;

public sealed class DashboardService(IDashboardStore store, IAuthorizationService authorization,
    IBusinessDateService businessDate, AlertRuleOptions alertOptions) : IDashboardQueryService
{
    private static readonly string[] RequiredTypes = ["PURCHASE", "SALE", "OUTSOURCE"];

    public async Task<DashboardSnapshot> LoadAsync(DashboardFilter filter, SecuritySection section, CancellationToken ct)
    {
        if (section is not (SecuritySection.Dashboard or SecuritySection.Kpi))
            throw new ArgumentOutOfRangeException(nameof(section));
        await authorization.RequireAsync(section, SecurityAction.View, ct);
        filter.Validate();
        DateOnly today = businessDate.Today;
        DashboardSourceSnapshot source = await store.LoadAsync(filter, today,
            alertOptions.ContractWarningDays, alertOptions.GuaranteeWarningDays, ct);
        return Build(source, filter, today, alertOptions.ContractWarningDays, alertOptions.GuaranteeWarningDays);
    }

    public static DashboardSnapshot Build(DashboardSourceSnapshot source, DashboardFilter filter,
        DateOnly today, int contractWarningDays, int guaranteeWarningDays)
    {
        List<DashboardContractDetail> contracts = source.Contracts.Select(f => ToDetail(f, today)).ToList();
        Dictionary<long, DashboardContractFact> factsById = source.Contracts.ToDictionary(x => x.Id);
        long Sum(Func<DashboardContractFact, long> selector) => source.Contracts.Aggregate(0L, (v, x) => checked(v + selector(x)));
        long claims = Sum(x => x.TotalClaimsRial), settledClaims = Sum(x => x.SettledClaimsRial);
        long debts = Sum(x => x.TotalDebtsRial), settledDebts = Sum(x => x.SettledDebtsRial);
        long payments = Sum(x => x.TotalPaymentsRial), receipts = Sum(x => x.TotalReceiptsRial);
        long receivables = FinancialProjectionPolicy.Remaining(claims, settledClaims), payables = FinancialProjectionPolicy.Remaining(debts, settledDebts);
        DateOnly contractLimit = today.AddDays(contractWarningDays), guaranteeLimit = today.AddDays(guaranteeWarningDays);
        List<DashboardContractDetail> expiringContracts = contracts.Where(x =>
            factsById[x.ContractId].StatusCode == "ACTIVE" &&
            x.EffectiveEndDate >= today && x.EffectiveEndDate <= contractLimit).ToList();
        DashboardGuaranteeFact[] expiringGuarantees = source.Guarantees.Where(x => x.StatusCode == "ACTIVE" && x.ExpiryDate >= today && x.ExpiryDate <= guaranteeLimit).ToArray();
        List<DashboardAlertDetail> openAlerts = source.Alerts.Where(x => x.StatusCode == "OPEN")
            .OrderByDescending(x => x.PriorityCode == "CRITICAL").ThenBy(x => x.DueDate).ThenBy(x => x.Id)
            .Select(x => new DashboardAlertDetail(x.Id, x.ContractId, x.ContractNo, x.Title, x.PriorityCode, x.PriorityTitle, x.DueDate)).ToList();
        List<DashboardContractDetail> critical = contracts.Where(x => x.IsCritical).ToList();
        List<DashboardContractDetail> overdueReceivables = contracts.Where(x => x.OverdueReceivableRial > 0).ToList();
        List<DashboardContractDetail> overduePayables = contracts.Where(x => x.OverduePayableRial > 0).ToList();

        List<DashboardMetric> metrics =
        [
            new("ACTIVE_CONTRACTS", "قرارداد فعال", source.Contracts.Count(x => x.StatusCode == "ACTIVE"), "count", DashboardDrilldownKind.ActiveContracts),
            new("FINAL_VALUE", "ارزش نهایی", Sum(x => x.FinalValueRial), "IRR", DashboardDrilldownKind.FinalValue),
            new("PAYMENTS", "پرداخت", payments, "IRR", DashboardDrilldownKind.Payments),
            new("RECEIPTS", "دریافت", receipts, "IRR", DashboardDrilldownKind.Receipts),
            new("RECEIVABLES", "مانده طلب", receivables, "IRR", DashboardDrilldownKind.Receivables),
            new("PAYABLES", "مانده بدهی", payables, "IRR", DashboardDrilldownKind.Payables),
            new("EXPIRING_CONTRACTS", "قرارداد رو به انقضا", expiringContracts.Count, "count", DashboardDrilldownKind.ExpiringContracts),
            new("EXPIRING_GUARANTEES", "ضمانت‌نامه رو به انقضا", expiringGuarantees.Length, "count", DashboardDrilldownKind.ExpiringGuarantees),
            new("OPEN_ALERTS", "هشدار باز", openAlerts.Count, "count", DashboardDrilldownKind.OpenAlerts)
        ];
        List<DashboardRatio> advanced =
        [
            Money("OVERDUE_RECEIVABLES", "مطالبات سررسیدگذشته", Sum(x => x.OverdueReceivableRial),DashboardDrilldownKind.OverdueReceivables),
            Money("OVERDUE_PAYABLES", "بدهی‌های سررسیدگذشته", Sum(x => x.OverduePayableRial),DashboardDrilldownKind.OverduePayables),
            Ratio("COLLECTION_RATE", "نرخ وصول", settledClaims, claims,DashboardDrilldownKind.CollectionRate),
            Ratio("PAYMENT_RATE", "نرخ پرداخت", settledDebts, debts,DashboardDrilldownKind.PaymentRate),
            Money("NET_CASH", "خالص جریان نقد", checked(receipts - payments),DashboardDrilldownKind.NetCash),
            Average("BUDGET_CONSUMPTION", "مصرف بودجه", source.Budgets.Where(x => x.ApprovedAmountMinor > 0).Select(x => 100m * x.ConsumedAmountMinor / x.ApprovedAmountMinor),DashboardDrilldownKind.BudgetConsumption),
            Average("AVERAGE_CONTRACTOR_SCORE", "میانگین ارزیابی پیمانکار", source.Evaluations.Select(x =>
                global::Contracts.Domain.Contracts.EvaluationScorePolicy.Total(x.Quality, x.Time, x.Financial, x.Hse)),DashboardDrilldownKind.AverageContractorScore)
        ];
        IReadOnlyList<ContractTypeKpi> types = RequiredTypes.Select(code =>
        {
            DashboardContractFact[] rows = source.Contracts.Where(x => x.TypeCode == code).ToArray();
            return new ContractTypeKpi(code, rows.FirstOrDefault()?.TypeTitle ?? code, rows.Length,
                rows.Count(x => x.StatusCode == "ACTIVE"), SumRows(rows, x => x.FinalValueRial),
                SumRows(rows, x => FinancialProjectionPolicy.Remaining(x.TotalClaimsRial,x.SettledClaimsRial)),
                SumRows(rows, x => FinancialProjectionPolicy.Remaining(x.TotalDebtsRial,x.SettledDebtsRial)),
                rows.Count(x => ToDetail(x, today).IsCritical));
        }).ToArray();

        List<DashboardContractDetail> topReceivables = contracts.Where(x => x.ReceivableRial > 0)
            .OrderByDescending(x => x.ReceivableRial).ThenBy(x => x.ContractNo, StringComparer.Ordinal).ThenBy(x => x.ContractId).Take(5).ToList();
        List<DashboardContractDetail> topPayables = contracts.Where(x => x.PayableRial > 0)
            .OrderByDescending(x => x.PayableRial).ThenBy(x => x.ContractNo, StringComparer.Ordinal).ThenBy(x => x.ContractId).Take(5).ToList();
        Dictionary<DashboardDrilldownKind, DashboardDrilldown> drilldowns = new()
        {
            [DashboardDrilldownKind.ActiveContracts] = Drill(DashboardDrilldownKind.ActiveContracts, contracts.Where((_, i) => source.Contracts[i].StatusCode == "ACTIVE")),
            [DashboardDrilldownKind.FinalValue] = Drill(DashboardDrilldownKind.FinalValue, contracts, x => x.FinalValueRial),
            [DashboardDrilldownKind.Payments] = Drill(DashboardDrilldownKind.Payments, contracts, (_, i) => source.Contracts[i].TotalPaymentsRial),
            [DashboardDrilldownKind.Receipts] = Drill(DashboardDrilldownKind.Receipts, contracts, (_, i) => source.Contracts[i].TotalReceiptsRial),
            [DashboardDrilldownKind.Receivables] = Drill(DashboardDrilldownKind.Receivables, contracts.Where(x => x.ReceivableRial > 0), x => x.ReceivableRial),
            [DashboardDrilldownKind.Payables] = Drill(DashboardDrilldownKind.Payables, contracts.Where(x => x.PayableRial > 0), x => x.PayableRial),
            [DashboardDrilldownKind.ExpiringContracts] = Drill(DashboardDrilldownKind.ExpiringContracts, expiringContracts),
            [DashboardDrilldownKind.ExpiringGuarantees] = new(DashboardDrilldownKind.ExpiringGuarantees, expiringGuarantees.Length, 0, [], [], expiringGuarantees.Select(x=>new DashboardEvidenceDetail("Guarantee",x.Id,$"{x.Number} — {x.ExpiryDate:yyyy-MM-dd}",x.ContractNo,x.AmountRial,0,null)).ToArray()),
            [DashboardDrilldownKind.OpenAlerts] = new(DashboardDrilldownKind.OpenAlerts, openAlerts.Count, 0, [], openAlerts, []),
            [DashboardDrilldownKind.OverdueReceivables] = Drill(DashboardDrilldownKind.OverdueReceivables, overdueReceivables, x => x.OverdueReceivableRial),
            [DashboardDrilldownKind.OverduePayables] = Drill(DashboardDrilldownKind.OverduePayables, overduePayables, x => x.OverduePayableRial),
            [DashboardDrilldownKind.CriticalContracts] = Drill(DashboardDrilldownKind.CriticalContracts, critical),
            [DashboardDrilldownKind.CollectionRate] = Drill(DashboardDrilldownKind.CollectionRate,contracts,x=>x.SettledClaimsRial),
            [DashboardDrilldownKind.PaymentRate] = Drill(DashboardDrilldownKind.PaymentRate,contracts,x=>x.SettledDebtsRial),
            [DashboardDrilldownKind.NetCash] = Drill(DashboardDrilldownKind.NetCash,contracts,x=>checked(x.TotalReceiptsRial-x.TotalPaymentsRial)),
            [DashboardDrilldownKind.BudgetConsumption] = new(DashboardDrilldownKind.BudgetConsumption,source.Budgets.Count(x=>x.ApprovedAmountMinor>0),0,[],[],source.Budgets.Where(x=>x.ApprovedAmountMinor>0).Select(x=>new DashboardEvidenceDetail("ProjectBudget",x.Id,$"{x.ProjectCode} — {x.ProjectTitle} — {x.FiscalYear} ({x.CurrencyCode})",null,x.ConsumedAmountMinor,x.ApprovedAmountMinor,decimal.Round(100m*x.ConsumedAmountMinor/x.ApprovedAmountMinor,2))).ToArray()),
            [DashboardDrilldownKind.AverageContractorScore] = new(DashboardDrilldownKind.AverageContractorScore,source.Evaluations.Count,0,[],[],source.Evaluations.Select(x=>new DashboardEvidenceDetail("ContractorEvaluation",x.Id,$"{x.ContractNo} — {x.Date:yyyy-MM-dd}",x.ContractNo,0,0,global::Contracts.Domain.Contracts.EvaluationScorePolicy.Total(x.Quality,x.Time,x.Financial,x.Hse))).ToArray())
        };
        return new(Guid.NewGuid(), source.CapturedAtUtc, today, filter, FilterSummary(filter, today),
            source.QueryCount, source.ElapsedMilliseconds, metrics, advanced, types, topReceivables,
            topPayables, openAlerts.Take(10).ToArray(), drilldowns);
    }

    private static DashboardContractDetail ToDetail(DashboardContractFact x, DateOnly today)
    {
        long receivable = FinancialProjectionPolicy.Remaining(x.TotalClaimsRial,x.SettledClaimsRial);
        long payable = FinancialProjectionPolicy.Remaining(x.TotalDebtsRial,x.SettledDebtsRial);
        bool critical = CriticalContractPolicy.IsCritical(x.StatusCode == "ACTIVE", x.EffectiveEndDate,
            today, x.OverdueReceivableRial, x.OverduePayableRial, x.CriticalOpenAlertCount);
        return new(x.Id, x.ContractNo, x.Title, x.TypeCode, x.TypeTitle, x.PartyName, x.ProjectTitle,
            x.StatusTitle, x.EffectiveEndDate, x.FinalValueRial, receivable, payable,x.TotalClaimsRial,x.SettledClaimsRial,x.TotalDebtsRial,x.SettledDebtsRial,x.TotalPaymentsRial,x.TotalReceiptsRial,
            x.OverdueReceivableRial, x.OverduePayableRial, critical);
    }
    private static DashboardRatio Ratio(string code, string title, long numerator, long denominator,DashboardDrilldownKind drilldown) => denominator == 0
        ? new(code, title, null, KpiValueStatus.NotApplicable, "percent",drilldown)
        : new(code, title, decimal.Round(100m * numerator / denominator, 2), KpiValueStatus.Applicable, "percent",drilldown);
    private static DashboardRatio Money(string code, string title, long value,DashboardDrilldownKind drilldown) => new(code, title, value, KpiValueStatus.Applicable, "IRR",drilldown);
    private static DashboardRatio Average(string code, string title, IEnumerable<decimal> values,DashboardDrilldownKind drilldown)
    { decimal[] rows = values.ToArray(); return rows.Length == 0 ? new(code, title, null, KpiValueStatus.NotApplicable, "percent",drilldown) : new(code, title, decimal.Round(rows.Average(), 2), KpiValueStatus.Applicable, "percent",drilldown); }
    private static long SumRows(IEnumerable<DashboardContractFact> rows, Func<DashboardContractFact, long> selector) => rows.Aggregate(0L, (v, x) => checked(v + selector(x)));
    private static DashboardDrilldown Drill(DashboardDrilldownKind kind, IEnumerable<DashboardContractDetail> rows, Func<DashboardContractDetail, long>? selector = null)
    { DashboardContractDetail[] all = rows.ToArray(); return new(kind, all.Length, selector is null ? 0 : all.Aggregate(0L, (v, x) => checked(v + selector(x))), all, [],[]); }
    private static DashboardDrilldown Drill(DashboardDrilldownKind kind, List<DashboardContractDetail> rows, Func<DashboardContractDetail, int, long> selector)
        => new(kind, rows.Count, rows.Select(selector).Aggregate(0L, checked((a, b) => a + b)), rows, [],[]);
    private static string FilterSummary(DashboardFilter f, DateOnly today)
    {
        static string Date(DateOnly? value) => value?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) ?? "ALL";
        static string Id(long? value) => value?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "ALL";
        return $"DateFrom={Date(f.DateFrom)}; DateTo={Date(f.DateTo)}; Type={Id(f.ContractTypeId)}; Status={Id(f.ContractStatusId)}; Party={Id(f.PartyId)}; Project={Id(f.ProjectId)}; BusinessDate={today.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)}; Timezone=Asia/Tehran; Snapshot=SQLite serializable";
    }
}
