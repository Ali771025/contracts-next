namespace Contracts.Infrastructure.Dashboard;

using System.Data;
using Contracts.Application.Dashboard;

public sealed class DashboardStore(IDbContextFactory<ContractsDbContext> factory) : IDashboardStore
{
    public async Task<DashboardSourceSnapshot> LoadAsync(DashboardFilter filter, DateOnly today,
        int contractWarningDays, int guaranteeWarningDays, CancellationToken ct)
    {
        _ = contractWarningDays; _ = guaranteeWarningDays;
        Stopwatch timer = Stopwatch.StartNew();
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        int queryCount = 0;
        IQueryable<Contract> selected = db.Contracts.AsNoTracking().Where(c => !c.IsDeleted &&
            (!filter.DateFrom.HasValue || c.StartDate >= filter.DateFrom) &&
            (!filter.DateTo.HasValue || c.StartDate <= filter.DateTo) &&
            (!filter.ContractTypeId.HasValue || c.ContractTypeId == filter.ContractTypeId) &&
            (!filter.ContractStatusId.HasValue || c.ContractStatusId == filter.ContractStatusId) &&
            (!filter.PartyId.HasValue || c.PartyId == filter.PartyId) &&
            (!filter.ProjectId.HasValue || c.ProjectId == filter.ProjectId));

        List<DashboardContractFact> contracts = await (
            from c in selected
            join type in db.Set<ContractType>().AsNoTracking() on c.ContractTypeId equals type.Id
            join status in db.Set<ContractStatus>().AsNoTracking() on c.ContractStatusId equals status.Id
            join party in db.Parties.AsNoTracking() on c.PartyId equals party.Id
            join project in db.Projects.AsNoTracking() on c.ProjectId equals project.Id into projects
            from project in projects.DefaultIfEmpty()
            let amendmentAmount = db.ContractAmendments.Where(a => !a.IsDeleted && a.ContractId == c.Id).Sum(a => (long?)a.AmountChangeRial) ?? 0
            let extensionDays = db.ContractAmendments.Where(a => !a.IsDeleted && a.ContractId == c.Id).Sum(a => (int?)a.TimeExtensionDays) ?? 0
            let claims = db.FinancialTransactions.Where(x => !x.IsDeleted && x.ContractId == c.Id && x.TransactionTypeId == ReferenceIds.TransactionTypes.Claim &&
                (!filter.DateFrom.HasValue || x.TransactionDate >= filter.DateFrom) && (!filter.DateTo.HasValue || x.TransactionDate <= filter.DateTo)).Sum(x => (long?)x.AmountRial) ?? 0
            let debts = db.FinancialTransactions.Where(x => !x.IsDeleted && x.ContractId == c.Id && x.TransactionTypeId == ReferenceIds.TransactionTypes.Debt &&
                (!filter.DateFrom.HasValue || x.TransactionDate >= filter.DateFrom) && (!filter.DateTo.HasValue || x.TransactionDate <= filter.DateTo)).Sum(x => (long?)x.AmountRial) ?? 0
            let settledClaims = (from a in db.ReceiptAllocations where !a.FinancialTransaction!.IsDeleted && !a.Receipt!.IsDeleted &&
                    a.FinancialTransaction.ContractId == c.Id && a.FinancialTransaction.TransactionTypeId == ReferenceIds.TransactionTypes.Claim &&
                    (!filter.DateFrom.HasValue || a.FinancialTransaction.TransactionDate >= filter.DateFrom) && (!filter.DateTo.HasValue || a.FinancialTransaction.TransactionDate <= filter.DateTo)
                select (long?)a.AmountRial).Sum() ?? 0
            let settledDebts = (from a in db.PaymentAllocations where !a.FinancialTransaction!.IsDeleted && !a.Payment!.IsDeleted &&
                    a.FinancialTransaction.ContractId == c.Id && a.FinancialTransaction.TransactionTypeId == ReferenceIds.TransactionTypes.Debt &&
                    (!filter.DateFrom.HasValue || a.FinancialTransaction.TransactionDate >= filter.DateFrom) && (!filter.DateTo.HasValue || a.FinancialTransaction.TransactionDate <= filter.DateTo)
                select (long?)a.AmountRial).Sum() ?? 0
            let payments = db.Payments.Where(x => !x.IsDeleted && x.ContractId == c.Id && (!filter.DateFrom.HasValue || x.PaymentDate >= filter.DateFrom) && (!filter.DateTo.HasValue || x.PaymentDate <= filter.DateTo)).Sum(x => (long?)x.AmountRial) ?? 0
            let receipts = db.Receipts.Where(x => !x.IsDeleted && x.ContractId == c.Id && (!filter.DateFrom.HasValue || x.ReceiptDate >= filter.DateFrom) && (!filter.DateTo.HasValue || x.ReceiptDate <= filter.DateTo)).Sum(x => (long?)x.AmountRial) ?? 0
            let overdueClaims = (from x in db.FinancialTransactions where !x.IsDeleted && x.ContractId == c.Id && x.TransactionTypeId == ReferenceIds.TransactionTypes.Claim && x.DueDate < today &&
                    (!filter.DateFrom.HasValue || x.TransactionDate >= filter.DateFrom) && (!filter.DateTo.HasValue || x.TransactionDate <= filter.DateTo)
                let allocated = db.ReceiptAllocations.Where(a => !a.Receipt!.IsDeleted && a.FinancialTransactionId == x.Id).Sum(a => (long?)a.AmountRial) ?? 0
                select (long?)(x.AmountRial - allocated)).Sum() ?? 0
            let overdueDebts = (from x in db.FinancialTransactions where !x.IsDeleted && x.ContractId == c.Id && x.TransactionTypeId == ReferenceIds.TransactionTypes.Debt && x.DueDate < today &&
                    (!filter.DateFrom.HasValue || x.TransactionDate >= filter.DateFrom) && (!filter.DateTo.HasValue || x.TransactionDate <= filter.DateTo)
                let allocated = db.PaymentAllocations.Where(a => !a.Payment!.IsDeleted && a.FinancialTransactionId == x.Id).Sum(a => (long?)a.AmountRial) ?? 0
                select (long?)(x.AmountRial - allocated)).Sum() ?? 0
            let criticalAlerts = db.Alerts.Count(a => !a.IsDeleted && a.ContractId == c.Id && a.AlertStatusId == 1 && a.PriorityId == 4)
            orderby c.NormalizedContractNo, c.Id
            select new DashboardContractFact(c.Id, c.ContractNo, c.Title, type.Code, type.TitleFa,
                status.Code, status.TitleFa, party.Name, project == null ? null : project.Title,
                c.EndDate.AddDays(extensionDays), c.BaseAmountRial + amendmentAmount, claims,
                settledClaims, debts, settledDebts, payments, receipts, overdueClaims,
                overdueDebts, criticalAlerts)).Take(filter.MaxContracts + 1).ToListAsync(ct);
        queryCount++;
        if (contracts.Count > filter.MaxContracts) throw new DashboardDataLimitExceededException(filter.MaxContracts);

        long[] contractIds = contracts.Select(x => x.Id).ToArray();
        List<DashboardGuaranteeFact> guarantees = await (from g in db.Guarantees.AsNoTracking()
            where !g.IsDeleted && contractIds.Contains(g.ContractId)
            join c in db.Contracts.AsNoTracking() on g.ContractId equals c.Id
            join s in db.Set<GuaranteeStatus>().AsNoTracking() on g.GuaranteeStatusId equals s.Id
            select new DashboardGuaranteeFact(g.Id,g.ContractId,c.ContractNo,g.GuaranteeNo,g.ExpiryDate,g.AmountRial,s.Code)).ToListAsync(ct); queryCount++;
        List<DashboardAlertFact> alerts = await (from a in db.Alerts.AsNoTracking()
            where !a.IsDeleted && contractIds.Contains(a.ContractId)
            join c in db.Contracts.AsNoTracking() on a.ContractId equals c.Id
            join p in db.Set<Priority>().AsNoTracking() on a.PriorityId equals p.Id
            join s in db.Set<AlertStatus>().AsNoTracking() on a.AlertStatusId equals s.Id
            orderby a.DueDate, a.Id
            select new DashboardAlertFact(a.Id, a.ContractId, c.ContractNo, a.Title, p.Code, p.TitleFa, s.Code, a.DueDate)).ToListAsync(ct); queryCount++;
        List<DashboardBudgetFact> budgets = await (from b in db.ProjectBudgets.AsNoTracking()
            where !b.IsDeleted && (!filter.ProjectId.HasValue || b.ProjectId == filter.ProjectId)
            join p in db.Projects.AsNoTracking() on b.ProjectId equals p.Id
            join c in db.Set<Currency>().AsNoTracking() on b.CurrencyId equals c.Id
            select new DashboardBudgetFact(b.Id,p.ProjectNo,p.Title,b.FiscalYear,c.Code,b.ApprovedAmountMinor,b.ConsumedAmountMinor)).ToListAsync(ct); queryCount++;
        List<DashboardEvaluationFact> evaluations = await (from e in db.ContractorEvaluations.AsNoTracking()
            where !e.IsDeleted && contractIds.Contains(e.ContractId) &&
                (!filter.DateFrom.HasValue || e.EvaluationDate >= filter.DateFrom) &&
                (!filter.DateTo.HasValue || e.EvaluationDate <= filter.DateTo)
            join c in db.Contracts.AsNoTracking() on e.ContractId equals c.Id
            select new DashboardEvaluationFact(e.Id,c.ContractNo,e.EvaluationDate,e.QualityScore, e.TimeScore, e.FinancialScore, e.HseScore)).ToListAsync(ct); queryCount++;
        await transaction.CommitAsync(ct);
        timer.Stop();
        return new(DateTimeOffset.UtcNow, queryCount, timer.ElapsedMilliseconds, contracts, guarantees, alerts, budgets, evaluations);
    }
}
