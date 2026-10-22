namespace Contracts.Infrastructure.Okr;

using System.Data;
using Contracts.Application.Okr;

public sealed class OkrStore(IDbContextFactory<ContractsDbContext> factory):IOkrStore
{
    public async Task<IReadOnlyList<OkrDefinitionRow>> DefinitionsAsync(CancellationToken ct)
    {
        await using ContractsDbContext db=await factory.CreateDbContextAsync(ct);
        return await (from kr in db.OkrKeyResults.AsNoTracking() where kr.IsActive
            join o in db.OkrObjectives.AsNoTracking().Where(x=>x.IsActive) on kr.ObjectiveId equals o.Id
            orderby o.SortOrder,kr.SortOrder select new OkrDefinitionRow(kr.Id,o.Id,o.Code,o.TitleFa,o.Description,o.SortOrder,
                kr.Code,kr.TitleFa,kr.TargetValue,kr.Unit,kr.WeightPercent,kr.Direction,kr.MetricCode,kr.DataSourceType,kr.MetricVersion,kr.SortOrder)).ToListAsync(ct);
    }
    public async Task<IReadOnlyDictionary<long,OkrMeasurementDto>> LatestMeasurementsAsync(CancellationToken ct)
    {
        await using ContractsDbContext db=await factory.CreateDbContextAsync(ct);
        List<OkrMeasurement> rows=await db.OkrMeasurements.AsNoTracking().OrderByDescending(x=>x.MeasurementDate).ThenByDescending(x=>x.Id).ToListAsync(ct);
        return rows.GroupBy(x=>x.KeyResultId).ToDictionary(g=>g.Key,g=>{OkrMeasurement m=g.First();return new OkrMeasurementDto(m.Id,m.ConcurrencyVersion,m.MeasurementDate,m.ActualValue,m.Score,m.SourceType,m.MetricVersion,m.AsOfUtc,m.EvidenceCount,m.Note,m.CreatedAtUtc,m.CreatedByUserId);});
    }
    public async Task AddMeasurementAsync(long keyResultId,DateOnly measurementDate,decimal actual,decimal score,OkrDataSourceType source,string metricVersion,string note,DateTimeOffset? asOfUtc,long? evidenceCount,AuditStamp stamp,CancellationToken ct)
    {
        await using ContractsDbContext db=await factory.CreateDbContextAsync(ct);
        OkrKeyResult definition=await db.OkrKeyResults.SingleAsync(x=>x.Id==keyResultId&&x.IsActive,ct);
        if(definition.DataSourceType!=source||definition.MetricVersion!=metricVersion)throw new DbUpdateConcurrencyException("OKR source or formula version changed; reload before measuring.");
        db.OkrMeasurements.Add(new(keyResultId,measurementDate,actual,score,source,metricVersion,note,asOfUtc,evidenceCount,stamp));await db.SaveChangesAsync(ct);
    }
    public async Task<IReadOnlyList<AutoMetricValue>> ReadLedgerMetricsAsync(DateOnly today,DateTimeOffset asOfUtc,CancellationToken ct)
    {
        await using ContractsDbContext db=await factory.CreateDbContextAsync(ct);await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        var obligations=await db.FinancialTransactions.AsNoTracking().Where(x=>!x.IsDeleted).Select(x=>new{x.Id,x.TransactionTypeId,x.DueDate,x.AmountRial}).ToListAsync(ct);
        var receipts=await (from a in db.ReceiptAllocations.AsNoTracking() where !a.Receipt!.IsDeleted&&!a.FinancialTransaction!.IsDeleted&&a.FinancialTransaction.TransactionTypeId==ReferenceIds.TransactionTypes.Claim select new{a.FinancialTransactionId,a.AmountRial}).ToListAsync(ct);
        var payments=await (from a in db.PaymentAllocations.AsNoTracking() where !a.Payment!.IsDeleted&&!a.FinancialTransaction!.IsDeleted&&a.FinancialTransaction.TransactionTypeId==ReferenceIds.TransactionTypes.Debt select new{a.FinancialTransactionId,a.AmountRial,a.Payment!.PaymentDate}).ToListAsync(ct);
        await tx.CommitAsync(ct);
        var claims=obligations.Where(x=>x.TransactionTypeId==ReferenceIds.TransactionTypes.Claim).ToArray();var debts=obligations.Where(x=>x.TransactionTypeId==ReferenceIds.TransactionTypes.Debt).ToArray();
        long claimTotal=Sum(claims.Select(x=>x.AmountRial));long collected=Sum(receipts.Select(x=>x.AmountRial));
        long overdue=Sum(claims.Where(x=>x.DueDate<today).Select(x=>Math.Max(0,x.AmountRial-Sum(receipts.Where(a=>a.FinancialTransactionId==x.Id).Select(a=>a.AmountRial)))));
        long debtTotal=Sum(debts.Select(x=>x.AmountRial));long timely=Sum(payments.Where(a=>debts.Any(d=>d.Id==a.FinancialTransactionId&&a.PaymentDate<=d.DueDate)).Select(a=>a.AmountRial));
        decimal? Rate(long n,long d)=>d==0?null:decimal.Round(100m*n/d,6,MidpointRounding.AwayFromZero);
        return [new("OVERDUE_RECEIVABLE_RATE",Rate(overdue,claimTotal),"LEDGER_V1",asOfUtc,claims.LongLength,$"claims={claims.Length}; denominator-rial={claimTotal}"),
            new("TIMELY_DEBT_PAYMENT_RATE",Rate(timely,debtTotal),"LEDGER_V1",asOfUtc,debts.LongLength,$"debts={debts.Length}; denominator-rial={debtTotal}"),
            new("COLLECTION_RATE",Rate(collected,claimTotal),"LEDGER_V1",asOfUtc,claims.LongLength,$"claims={claims.Length}; denominator-rial={claimTotal}")];
    }
    private static long Sum(IEnumerable<long> values)=>values.Aggregate(0L,(a,b)=>checked(a+b));
}
