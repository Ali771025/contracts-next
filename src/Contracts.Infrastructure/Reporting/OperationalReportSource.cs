namespace Contracts.Infrastructure.Reporting;

using System.Data;
using Contracts.Application.Reporting;
using Contracts.Infrastructure.Operations;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class OperationalReportSource(IDbContextFactory<ContractsDbContext> factory,DocumentStorageOptions storage):IOperationalReportSource
{
    public async Task<OperationalReportSnapshot> LoadAsync(ReportFilter filter,DateOnly businessDate,CancellationToken ct)
    {
        filter.Validate();await using ContractsDbContext db=await factory.CreateDbContextAsync(ct);
        await using IDbContextTransaction tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        OperationalReportSnapshot result=filter.Kind switch
        {
            ReportKind.Guarantees=>Empty() with{Guarantees=await Guarantees(db,filter,ct)},
            ReportKind.ExpiringContracts=>Empty() with{ExpiringContracts=await Expiring(db,filter,ct)},
            ReportKind.ProjectFinance=>Empty() with{ProjectFinance=await Projects(db,filter,ct)},
            ReportKind.ContractorEvaluation=>Empty() with{Evaluations=await Evaluations(db,filter,ct)},
            ReportKind.Invoices=>Empty() with{Invoices=await Invoices(db,filter,ct)},
            ReportKind.Amendments=>Empty() with{Amendments=await Amendments(db,filter,ct)},
            ReportKind.Documents=>Empty() with{Documents=await Documents(db,filter,ct)},
            ReportKind.Alerts=>Empty() with{Alerts=await Alerts(db,filter,ct)},
            _=>Empty()
        };
        await tx.CommitAsync(ct);return result;
    }

    private static IQueryable<Contract> Contracts(ContractsDbContext db,ReportFilter f)=>db.Contracts.AsNoTracking().Where(c=>!c.IsDeleted&&
        (!f.ContractTypeId.HasValue||c.ContractTypeId==f.ContractTypeId)&&(!f.ContractStatusId.HasValue||c.ContractStatusId==f.ContractStatusId)&&
        (!f.PartyId.HasValue||c.PartyId==f.PartyId)&&(!f.ProjectId.HasValue||c.ProjectId==f.ProjectId));

    private static Task<List<GuaranteeReportSource>> Guarantees(ContractsDbContext db,ReportFilter f,CancellationToken ct)=>(from g in db.Guarantees.AsNoTracking() where !g.IsDeleted&&(!f.DateFrom.HasValue||g.IssueDate>=f.DateFrom)&&(!f.DateTo.HasValue||g.IssueDate<=f.DateTo)
        join c in Contracts(db,f) on g.ContractId equals c.Id join p in db.Parties.AsNoTracking() on c.PartyId equals p.Id
        join t in db.Set<GuaranteeType>().AsNoTracking() on g.GuaranteeTypeId equals t.Id join s in db.Set<GuaranteeStatus>().AsNoTracking() on g.GuaranteeStatusId equals s.Id
        join b0 in db.Set<Bank>().AsNoTracking() on g.BankId equals b0.Id into banks from b in banks.DefaultIfEmpty()
        orderby g.ExpiryDate,g.Id select new GuaranteeReportSource(g.Id,c.Id,c.ContractNo,p.Name,t.TitleFa,g.GuaranteeNo,b==null?null:b.TitleFa,g.IssueDate,g.ExpiryDate,g.AmountRial,s.TitleFa)).Take(f.MaxRows).ToListAsync(ct);

    private static Task<List<ExpiringContractReportSource>> Expiring(ContractsDbContext db,ReportFilter f,CancellationToken ct)=>(from c in Contracts(db,f)
        join p in db.Parties.AsNoTracking() on c.PartyId equals p.Id join t in db.Set<ContractType>().AsNoTracking() on c.ContractTypeId equals t.Id join s in db.Set<ContractStatus>().AsNoTracking() on c.ContractStatusId equals s.Id
        let extension=db.ContractAmendments.Where(a=>a.ContractId==c.Id&&!a.IsDeleted).Sum(a=>(int?)a.TimeExtensionDays)??0
        orderby c.EndDate,c.Id select new ExpiringContractReportSource(c.Id,c.ContractNo,c.Title,p.Name,t.TitleFa,c.EndDate,extension,s.TitleFa)).Take(f.MaxRows).ToListAsync(ct);

    private static Task<List<ProjectFinanceReportSource>> Projects(ContractsDbContext db,ReportFilter f,CancellationToken ct)
    {IQueryable<long> projectIds=Contracts(db,f).Where(c=>c.ProjectId.HasValue).Select(c=>c.ProjectId!.Value);return(from b in db.ProjectBudgets.AsNoTracking() where !b.IsDeleted&&(!f.ProjectId.HasValue||b.ProjectId==f.ProjectId)&&(!HasContractFilter(f)||projectIds.Contains(b.ProjectId))
        join p in db.Projects.AsNoTracking() on b.ProjectId equals p.Id join c in db.Set<Currency>().AsNoTracking() on b.CurrencyId equals c.Id
        orderby p.NormalizedProjectNo,b.FiscalYear,b.Id select new ProjectFinanceReportSource(p.Id,p.ProjectNo,p.Title,b.FiscalYear,c.TitleFa,b.ApprovedAmountMinor,b.ConsumedAmountMinor)).Take(f.MaxRows).ToListAsync(ct);}

    private static Task<List<EvaluationReportSource>> Evaluations(ContractsDbContext db,ReportFilter f,CancellationToken ct)=>(from e in db.ContractorEvaluations.AsNoTracking() where !e.IsDeleted&&(!f.DateFrom.HasValue||e.EvaluationDate>=f.DateFrom)&&(!f.DateTo.HasValue||e.EvaluationDate<=f.DateTo)
        join c in Contracts(db,f) on e.ContractId equals c.Id join p in db.Parties.AsNoTracking() on e.PartyId equals p.Id orderby e.EvaluationDate,e.Id
        select new EvaluationReportSource(e.Id,c.Id,c.ContractNo,p.Name,e.EvaluationDate,e.QualityScore,e.TimeScore,e.FinancialScore,e.HseScore)).Take(f.MaxRows).ToListAsync(ct);

    private static Task<List<InvoiceReportSource>> Invoices(ContractsDbContext db,ReportFilter f,CancellationToken ct)=>(from i in db.Invoices.AsNoTracking() where !i.IsDeleted&&(!f.DateFrom.HasValue||i.InvoiceDate>=f.DateFrom)&&(!f.DateTo.HasValue||i.InvoiceDate<=f.DateTo)
        join c in Contracts(db,f) on i.ContractId equals c.Id join p in db.Parties.AsNoTracking() on c.PartyId equals p.Id join t in db.Set<InvoiceType>().AsNoTracking() on i.InvoiceTypeId equals t.Id join s in db.Set<PaymentStatus>().AsNoTracking() on i.PaymentStatusId equals s.Id
        orderby i.InvoiceDate,i.Id select new InvoiceReportSource(i.Id,c.Id,c.ContractNo,p.Name,i.InvoiceNo,i.InvoiceDate,t.TitleFa,i.AmountRial,i.VatAmountRial,s.TitleFa)).Take(f.MaxRows).ToListAsync(ct);

    private static Task<List<AmendmentReportSource>> Amendments(ContractsDbContext db,ReportFilter f,CancellationToken ct)=>(from a in db.ContractAmendments.AsNoTracking() where !a.IsDeleted&&(!f.DateFrom.HasValue||a.AmendmentDate>=f.DateFrom)&&(!f.DateTo.HasValue||a.AmendmentDate<=f.DateTo)
        join c in Contracts(db,f) on a.ContractId equals c.Id orderby a.AmendmentDate,a.Id select new AmendmentReportSource(a.Id,c.Id,c.ContractNo,a.AmendmentNo,a.AmendmentDate,a.Subject,a.AmountChangeRial,a.TimeExtensionDays)).Take(f.MaxRows).ToListAsync(ct);

    private async Task<List<DocumentReportSource>> Documents(ContractsDbContext db,ReportFilter f,CancellationToken ct)
    {List<DocumentProjection> raw=await(from d in db.ContractDocuments.AsNoTracking() join c in Contracts(db,f) on d.ContractId equals c.Id join t in db.Set<DocumentType>().AsNoTracking() on d.DocumentTypeId equals t.Id
        orderby d.CreatedAtUtc,d.Id select new DocumentProjection(d.Id,c.Id,c.ContractNo,t.TitleFa,d.OriginalFileName,d.RelativePath,d.FileSize,d.ContentHashSha256,d.CreatedAtUtc)).Take(f.MaxRows).ToListAsync(ct);
        return raw.Where(x=>InBusinessDateRange(x.UploadedAtUtc,f)).Select(x=>new DocumentReportSource(x.Id,x.ContractId,x.ContractNo,x.Type,x.OriginalName,x.Size,x.Hash,!ExistsSafely(x.RelativePath),x.UploadedAtUtc)).ToList();}

    private static Task<List<AlertReportSource>> Alerts(ContractsDbContext db,ReportFilter f,CancellationToken ct)=>(from a in db.Alerts.AsNoTracking() where !a.IsDeleted&&(!f.DateFrom.HasValue||a.DueDate>=f.DateFrom)&&(!f.DateTo.HasValue||a.DueDate<=f.DateTo)
        join c in Contracts(db,f) on a.ContractId equals c.Id join t in db.Set<AlertType>().AsNoTracking() on a.AlertTypeId equals t.Id join s in db.Set<AlertStatus>().AsNoTracking() on a.AlertStatusId equals s.Id join p in db.Set<Priority>().AsNoTracking() on a.PriorityId equals p.Id
        orderby a.DueDate,a.Id select new AlertReportSource(a.Id,c.Id,c.ContractNo,t.TitleFa,a.Title,a.DueDate,p.TitleFa,s.TitleFa,a.DeduplicationKey)).Take(f.MaxRows).ToListAsync(ct);

    private bool ExistsSafely(string relative){string root=Path.GetFullPath(storage.RootPath)+Path.DirectorySeparatorChar;string candidate=Path.GetFullPath(Path.Combine(root,relative));return candidate.StartsWith(root,StringComparison.OrdinalIgnoreCase)&&File.Exists(candidate);}
    private static bool InBusinessDateRange(DateTimeOffset utc,ReportFilter f){TimeZoneInfo zone=TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");DateOnly local=DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(utc,zone).DateTime);return(!f.DateFrom.HasValue||local>=f.DateFrom)&&(!f.DateTo.HasValue||local<=f.DateTo);}
    private static bool HasContractFilter(ReportFilter f)=>f.ContractTypeId.HasValue||f.ContractStatusId.HasValue||f.PartyId.HasValue;
    private static OperationalReportSnapshot Empty()=>new([],[],[],[],[],[],[],[]);
    private sealed record DocumentProjection(long Id,long ContractId,string ContractNo,string Type,string OriginalName,string RelativePath,long Size,string? Hash,DateTimeOffset UploadedAtUtc);
}
