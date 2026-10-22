namespace Contracts.Infrastructure.Finance;

using System.Data;
using Contracts.Application.Contracts;
using Contracts.Application.Finance;
using Contracts.Application.MasterData;
using Contracts.Application.Presentation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class FinanceStore(IDbContextFactory<ContractsDbContext> factory) : IFinanceStore
{
    public async Task<CanonicalFinancialReportSnapshot> ReportingSnapshotAsync(CanonicalReportFilter filter,DateOnly today,CancellationToken ct)
    {
        await using ContractsDbContext db=await factory.CreateDbContextAsync(ct);
        await using IDbContextTransaction transaction=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        List<CanonicalReportContract> contracts=await (from c in db.Contracts.AsNoTracking()
            where !c.IsDeleted&&(!filter.DateFrom.HasValue||c.StartDate>=filter.DateFrom)&&(!filter.DateTo.HasValue||c.StartDate<=filter.DateTo)&&(!filter.ContractTypeId.HasValue||c.ContractTypeId==filter.ContractTypeId)&&(!filter.ContractStatusId.HasValue||c.ContractStatusId==filter.ContractStatusId)&&(!filter.PartyId.HasValue||c.PartyId==filter.PartyId)&&(!filter.ProjectId.HasValue||c.ProjectId==filter.ProjectId)
            join type in db.Set<ContractType>().AsNoTracking() on c.ContractTypeId equals type.Id
            join status in db.Set<ContractStatus>().AsNoTracking() on c.ContractStatusId equals status.Id
            join party in db.Parties.AsNoTracking() on c.PartyId equals party.Id
            join projectValue in db.Projects.AsNoTracking() on c.ProjectId equals projectValue.Id into projects from project in projects.DefaultIfEmpty()
            let amendmentTotal=db.ContractAmendments.Where(a=>a.ContractId==c.Id&&!a.IsDeleted).Sum(a=>(long?)a.AmountChangeRial)??0
            orderby c.NormalizedContractNo,c.Id select new CanonicalReportContract(c.Id,c.ContractNo,c.Title,c.ContractTypeId,type.Code,type.TitleFa,c.ContractStatusId,status.TitleFa,c.PartyId,party.Name,c.ProjectId,project==null?null:project.Title,c.BaseAmountRial,c.BaseAmountRial+amendmentTotal)).Take(filter.MaxRows).ToListAsync(ct);
        long[] ids=contracts.Select(x=>x.Id).ToArray();
        if(filter.Purpose==CanonicalReportPurpose.Summary)
        {
            var obligationAmounts=await db.FinancialTransactions.AsNoTracking().Where(x=>!x.IsDeleted&&ids.Contains(x.ContractId)&&(!filter.DateFrom.HasValue||x.TransactionDate>=filter.DateFrom)&&(!filter.DateTo.HasValue||x.TransactionDate<=filter.DateTo)).GroupBy(x=>new{x.ContractId,x.TransactionTypeId}).Select(g=>new{g.Key.ContractId,TypeId=g.Key.TransactionTypeId,Amount=g.Sum(x=>x.AmountRial)}).ToListAsync(ct);
            var receiptAllocated=await(from a in db.ReceiptAllocations.AsNoTracking() join x in db.FinancialTransactions.AsNoTracking() on a.FinancialTransactionId equals x.Id where !x.IsDeleted&&x.TransactionTypeId==1&&ids.Contains(x.ContractId)&&!a.Receipt!.IsDeleted&&(!filter.DateFrom.HasValue||x.TransactionDate>=filter.DateFrom)&&(!filter.DateTo.HasValue||x.TransactionDate<=filter.DateTo) group a by x.ContractId into g select new{ContractId=g.Key,Amount=g.Sum(x=>x.AmountRial)}).ToDictionaryAsync(x=>x.ContractId,x=>x.Amount,ct);
            var paymentAllocated=await(from a in db.PaymentAllocations.AsNoTracking() join x in db.FinancialTransactions.AsNoTracking() on a.FinancialTransactionId equals x.Id where !x.IsDeleted&&x.TransactionTypeId==2&&ids.Contains(x.ContractId)&&!a.Payment!.IsDeleted&&(!filter.DateFrom.HasValue||x.TransactionDate>=filter.DateFrom)&&(!filter.DateTo.HasValue||x.TransactionDate<=filter.DateTo) group a by x.ContractId into g select new{ContractId=g.Key,Amount=g.Sum(x=>x.AmountRial)}).ToDictionaryAsync(x=>x.ContractId,x=>x.Amount,ct);
            List<CanonicalReportObligation> aggregateObligations=obligationAmounts.Select(x=>{long allocated=x.TypeId==1?receiptAllocated.GetValueOrDefault(x.ContractId):paymentAllocated.GetValueOrDefault(x.ContractId);return new CanonicalReportObligation(0,x.ContractId,x.TypeId,default,default,x.Amount,allocated,FinancialProjectionPolicy.Remaining(x.Amount,allocated));}).ToList();
            var paymentTotals=await db.Payments.AsNoTracking().Where(x=>!x.IsDeleted&&ids.Contains(x.ContractId)&&(!filter.DateFrom.HasValue||x.PaymentDate>=filter.DateFrom)&&(!filter.DateTo.HasValue||x.PaymentDate<=filter.DateTo)).GroupBy(x=>x.ContractId).Select(g=>new{ContractId=g.Key,Amount=g.Sum(x=>x.AmountRial)}).ToListAsync(ct);
            var receiptTotals=await db.Receipts.AsNoTracking().Where(x=>!x.IsDeleted&&ids.Contains(x.ContractId)&&(!filter.DateFrom.HasValue||x.ReceiptDate>=filter.DateFrom)&&(!filter.DateTo.HasValue||x.ReceiptDate<=filter.DateTo)).GroupBy(x=>x.ContractId).Select(g=>new{ContractId=g.Key,Amount=g.Sum(x=>x.AmountRial)}).ToListAsync(ct);
            List<CanonicalReportSettlement> aggregateSettlements=paymentTotals.Select(x=>new CanonicalReportSettlement(0,SettlementKind.Payment,x.ContractId,"",default,x.Amount,0,x.Amount,"",null,null)).Concat(receiptTotals.Select(x=>new CanonicalReportSettlement(0,SettlementKind.Receipt,x.ContractId,"",default,x.Amount,0,x.Amount,"",null,null))).ToList();
            await transaction.CommitAsync(ct);return new(contracts,aggregateObligations,aggregateSettlements);
        }
        List<ObligationProjection> obligationRows=[];
        List<CanonicalReportSettlement> settlements=[];
        if(filter.Purpose==CanonicalReportPurpose.Payments){List<SettlementListProjection> paymentRows=await (from x in db.Payments.AsNoTracking() where !x.IsDeleted&&ids.Contains(x.ContractId)&&(!filter.DateFrom.HasValue||x.PaymentDate>=filter.DateFrom)&&(!filter.DateTo.HasValue||x.PaymentDate<=filter.DateTo)
            join c in db.Contracts.AsNoTracking() on x.ContractId equals c.Id join pt in db.Set<PaymentType>().AsNoTracking() on x.PaymentTypeId equals pt.Id join u in db.Users.AsNoTracking() on x.CreatedByUserId equals u.Id join b0 in db.Set<Bank>().AsNoTracking() on x.BankId equals b0.Id into banks from b in banks.DefaultIfEmpty()
            let allocated=db.PaymentAllocations.Where(a=>a.PaymentId==x.Id).Sum(a=>(long?)a.AmountRial)??0 orderby x.PaymentDate,x.Id select new SettlementListProjection{Id=x.Id,ContractId=x.ContractId,Number=x.PaymentNo,Date=x.PaymentDate,Amount=x.AmountRial,Allocated=allocated,PaymentType=pt.TitleFa,Bank=b==null?null:b.TitleFa,Reference=x.ReferenceNo}).Take(filter.MaxRows).ToListAsync(ct);
        settlements.AddRange(paymentRows.Select(x=>new CanonicalReportSettlement(x.Id,SettlementKind.Payment,x.ContractId,x.Number,x.Date,x.Amount,x.Allocated,FinancialProjectionPolicy.Remaining(x.Amount,x.Allocated),x.PaymentType,x.Bank,x.Reference)));}
        if(filter.Purpose==CanonicalReportPurpose.Receipts){List<SettlementListProjection> receiptRows=await (from x in db.Receipts.AsNoTracking() where !x.IsDeleted&&ids.Contains(x.ContractId)&&(!filter.DateFrom.HasValue||x.ReceiptDate>=filter.DateFrom)&&(!filter.DateTo.HasValue||x.ReceiptDate<=filter.DateTo)
            join c in db.Contracts.AsNoTracking() on x.ContractId equals c.Id join pt in db.Set<PaymentType>().AsNoTracking() on x.PaymentTypeId equals pt.Id join u in db.Users.AsNoTracking() on x.CreatedByUserId equals u.Id join b0 in db.Set<Bank>().AsNoTracking() on x.BankId equals b0.Id into banks from b in banks.DefaultIfEmpty()
            let allocated=db.ReceiptAllocations.Where(a=>a.ReceiptId==x.Id).Sum(a=>(long?)a.AmountRial)??0 orderby x.ReceiptDate,x.Id select new SettlementListProjection{Id=x.Id,ContractId=x.ContractId,Number=x.ReceiptNo,Date=x.ReceiptDate,Amount=x.AmountRial,Allocated=allocated,PaymentType=pt.TitleFa,Bank=b==null?null:b.TitleFa,Reference=x.ReferenceNo}).Take(filter.MaxRows).ToListAsync(ct);
            settlements.AddRange(receiptRows.Select(x=>new CanonicalReportSettlement(x.Id,SettlementKind.Receipt,x.ContractId,x.Number,x.Date,x.Amount,x.Allocated,FinancialProjectionPolicy.Remaining(x.Amount,x.Allocated),x.PaymentType,x.Bank,x.Reference)));}
        await transaction.CommitAsync(ct);
        return new(contracts,obligationRows.Select(x=>new CanonicalReportObligation(x.Id,x.ContractId,x.TypeId,x.TransactionDate,x.DueDate,x.AmountRial,x.Allocated,x.Remaining)).ToList(),settlements.OrderBy(x=>x.Date).ThenBy(x=>x.Id).ToList());
    }

    public async Task<FinanceReferenceData> ReferencesAsync(CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        List<FinanceContractOption> contracts = await (from c in db.Contracts.AsNoTracking() where !c.IsDeleted
            join t in db.Set<ContractType>() on c.ContractTypeId equals t.Id
            join p in db.Parties on c.PartyId equals p.Id
            orderby c.NormalizedContractNo, c.Id
            select new FinanceContractOption(c.Id, c.ContractNo, c.Title, t.Code, t.TitleFa, p.Name)).ToListAsync(ct);
        return new(contracts, await Refs<TransactionType>(db, ct), await Refs<FinancialTransactionStatus>(db, ct),
            await Refs<PaymentType>(db, ct), await Refs<Bank>(db, ct));
    }

    public async Task<string?> ContractTypeCodeAsync(long contractId, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        return await (from c in db.Contracts.AsNoTracking() where c.Id == contractId && !c.IsDeleted
            join t in db.Set<ContractType>() on c.ContractTypeId equals t.Id select t.Code).SingleOrDefaultAsync(ct);
    }

    public async Task<PageResult<ObligationRow>> SearchObligationsAsync(LedgerFilter filter, DateOnly today, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        string search = TextNormalizer.SearchKey(filter.Search ?? string.Empty);
        IQueryable<FinancialTransaction> baseQuery = db.FinancialTransactions.AsNoTracking().Where(x => !x.IsDeleted &&
            (!filter.ContractId.HasValue || x.ContractId == filter.ContractId) &&
            (!filter.TransactionTypeId.HasValue || x.TransactionTypeId == filter.TransactionTypeId) &&
            (!filter.DueFrom.HasValue || x.DueDate >= filter.DueFrom) && (!filter.DueTo.HasValue || x.DueDate <= filter.DueTo));
        IQueryable<ObligationProjection> query = ProjectObligations(db, baseQuery, today)
            .Where(x => search == "" || x.TransactionNo.Contains(search) || x.ContractNo.Contains(search) || x.ContractTitle.Contains(search));
        if (filter.StatusId.HasValue) query = query.Where(x => x.StatusId == filter.StatusId);
        if (filter.IsOverdue.HasValue) query = query.Where(x => x.IsOverdue == filter.IsOverdue);
        int total = await query.CountAsync(ct);
        query = (filter.Sort.ToLowerInvariant(), filter.Descending) switch
        {
            ("amount", false) => query.OrderBy(x => x.AmountRial).ThenBy(x => x.Id),
            ("amount", true) => query.OrderByDescending(x => x.AmountRial).ThenByDescending(x => x.Id),
            ("remaining", false) => query.OrderBy(x => x.Remaining).ThenBy(x => x.Id),
            ("remaining", true) => query.OrderByDescending(x => x.Remaining).ThenByDescending(x => x.Id),
            (_, true) => query.OrderByDescending(x => x.DueDate).ThenByDescending(x => x.Id),
            _ => query.OrderBy(x => x.DueDate).ThenBy(x => x.Id),
        };
        List<ObligationProjection> rows = await query.Skip(filter.Skip).Take(filter.PageSize).ToListAsync(ct);
        return new(rows.Select(ToRow).ToList(), total, filter.Page, filter.PageSize);
    }

    public async Task<ObligationRow?> GetObligationAsync(long id, DateOnly today, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        ObligationProjection? item = await ProjectObligations(db,
            db.FinancialTransactions.AsNoTracking().Where(x => x.Id == id && !x.IsDeleted), today).SingleOrDefaultAsync(ct);
        return item is null ? null : ToRow(item);
    }

    public async Task<ObligationRow> CreateObligationAsync(ObligationInput input, AuditStamp stamp, DateOnly today, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        await using IDbContextTransaction tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await EnsureContractAndType(db, input.ContractId, input.TransactionTypeId, ct);
        FinancialTransaction item = new("FT-" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture).ToUpperInvariant(),
            input.TransactionTypeId == ReferenceIds.TransactionTypes.Claim ? "CLAIM" : "DEBT", input.ContractId,
            input.TransactionTypeId, 1, input.AmountRial, input.TransactionDate, input.DueDate, input.Description, stamp);
        db.FinancialTransactions.Add(item); await Save(db, ct); await tx.CommitAsync(ct);
        return (await GetObligationAsync(item.Id, today, ct))!;
    }

    public async Task<ObligationRow> UpdateObligationAsync(ObligationUpdate input, AuditStamp stamp, DateOnly today, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        await using IDbContextTransaction tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        FinancialTransaction item = await db.FinancialTransactions.Include(x => x.PaymentAllocations).Include(x => x.ReceiptAllocations)
            .SingleOrDefaultAsync(x => x.Id == input.Id && !x.IsDeleted, ct) ?? throw new KeyNotFoundException("Obligation was not found.");
        db.Entry(item).Property(x => x.ConcurrencyVersion).OriginalValue = input.ConcurrencyVersion;
        long status = StatusId(input.AmountRial, item.AllocatedAmountRial);
        item.Update(input.TransactionDate, input.DueDate, input.AmountRial, status, input.Description, stamp);
        await Save(db, ct); await tx.CommitAsync(ct); return (await GetObligationAsync(item.Id, today, ct))!;
    }

    public async Task DeleteObligationAsync(long id, long version, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        await using IDbContextTransaction tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        FinancialTransaction item = await db.FinancialTransactions.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Obligation was not found.");
        if (await db.PaymentAllocations.AnyAsync(x => x.FinancialTransactionId == id, ct) ||
            await db.ReceiptAllocations.AnyAsync(x => x.FinancialTransactionId == id, ct))
            throw new MasterDataDependencyException("An allocated obligation cannot be deleted.");
        db.Entry(item).Property(x => x.ConcurrencyVersion).OriginalValue = version; item.SoftDelete(stamp);
        await Save(db, ct); await tx.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<ObligationRow>> OpenObligationsAsync(long contractId, SettlementKind kind, DateOnly today, CancellationToken ct)
    {
        long type = kind == SettlementKind.Payment ? ReferenceIds.TransactionTypes.Debt : ReferenceIds.TransactionTypes.Claim;
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        List<ObligationProjection> rows = await ProjectObligations(db, db.FinancialTransactions.AsNoTracking()
            .Where(x => !x.IsDeleted && x.ContractId == contractId && x.TransactionTypeId == type), today)
            .Where(x => x.Remaining > 0).OrderBy(x => x.DueDate).ThenBy(x => x.Id).ToListAsync(ct);
        return rows.Select(ToRow).ToList();
    }

    public async Task<PageResult<SettlementRow>> SearchSettlementsAsync(SettlementKind kind, long? contractId, int page, int pageSize, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        int skip = checked((page - 1) * pageSize);
        if (kind == SettlementKind.Payment)
        {
            IQueryable<Payment> query = db.Payments.AsNoTracking().Where(x => !x.IsDeleted && (!contractId.HasValue || x.ContractId == contractId));
            int total = await query.CountAsync(ct);
            List<SettlementListProjection> rows = await (from x in query join c in db.Contracts.AsNoTracking() on x.ContractId equals c.Id
                join pt in db.Set<PaymentType>().AsNoTracking() on x.PaymentTypeId equals pt.Id join u in db.Users.AsNoTracking() on x.CreatedByUserId equals u.Id
                join b0 in db.Set<Bank>().AsNoTracking() on x.BankId equals b0.Id into banks from b in banks.DefaultIfEmpty()
                let allocated=db.PaymentAllocations.Where(a=>a.PaymentId==x.Id).Sum(a=>(long?)a.AmountRial)??0
                orderby x.PaymentDate descending,x.Id descending select new SettlementListProjection{Id=x.Id,Version=x.ConcurrencyVersion,ContractId=x.ContractId,ContractNo=c.ContractNo,Number=x.PaymentNo,Date=x.PaymentDate,Amount=x.AmountRial,Allocated=allocated,PaymentType=pt.TitleFa,Bank=b==null?null:b.TitleFa,Reference=x.ReferenceNo,InvoiceNo=x.InvoiceNo,ChequeNo=x.ChequeNo,Payer=x.PayerDescription,Description=x.Description,Attachment=x.AttachmentPath,ActorId=x.CreatedByUserId!.Value,Actor=u.DisplayName,At=x.CreatedAtUtc}).Skip(skip).Take(pageSize).ToListAsync(ct);
            return new(rows.Select(x=>ToSettlementRow(x,kind)).ToList(),total,page,pageSize);
        }
        else
        {
            IQueryable<Receipt> query = db.Receipts.AsNoTracking().Where(x => !x.IsDeleted && (!contractId.HasValue || x.ContractId == contractId));
            int total = await query.CountAsync(ct);
            List<SettlementListProjection> rows = await (from x in query join c in db.Contracts.AsNoTracking() on x.ContractId equals c.Id
                join pt in db.Set<PaymentType>().AsNoTracking() on x.PaymentTypeId equals pt.Id join u in db.Users.AsNoTracking() on x.CreatedByUserId equals u.Id
                join b0 in db.Set<Bank>().AsNoTracking() on x.BankId equals b0.Id into banks from b in banks.DefaultIfEmpty()
                let allocated=db.ReceiptAllocations.Where(a=>a.ReceiptId==x.Id).Sum(a=>(long?)a.AmountRial)??0
                orderby x.ReceiptDate descending,x.Id descending select new SettlementListProjection{Id=x.Id,Version=x.ConcurrencyVersion,ContractId=x.ContractId,ContractNo=c.ContractNo,Number=x.ReceiptNo,Date=x.ReceiptDate,Amount=x.AmountRial,Allocated=allocated,PaymentType=pt.TitleFa,Bank=b==null?null:b.TitleFa,Reference=x.ReferenceNo,InvoiceNo=x.InvoiceNo,ChequeNo=x.ChequeNo,Payer=x.PayerDescription,Description=x.Description,Attachment=x.AttachmentPath,ActorId=x.CreatedByUserId!.Value,Actor=u.DisplayName,At=x.CreatedAtUtc}).Skip(skip).Take(pageSize).ToListAsync(ct);
            return new(rows.Select(x=>ToSettlementRow(x,kind)).ToList(),total,page,pageSize);
        }
    }

    public async Task<SettlementRow?> GetSettlementAsync(SettlementKind kind, long id, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        return kind == SettlementKind.Payment ? await PaymentRow(db, id, ct) : await ReceiptRow(db, id, ct);
    }

    public Task<SettlementRow> CreateSettlementAsync(SettlementKind kind, SettlementInput input, AuditStamp stamp, CancellationToken ct) =>
        WriteSettlement(kind, null, input, stamp, ct);
    public Task<SettlementRow> UpdateSettlementAsync(SettlementKind kind, SettlementUpdate input, AuditStamp stamp, CancellationToken ct) =>
        WriteSettlement(kind, input, input.Values, stamp, ct);

    public async Task DeleteSettlementAsync(SettlementKind kind, long id, long version, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        await using IDbContextTransaction tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        HashSet<long> obligations = [];
        if (kind == SettlementKind.Payment)
        {
            Payment item = await db.Payments.Include(x => x.Allocations).ThenInclude(x => x.FinancialTransaction)
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct) ?? throw new KeyNotFoundException("Payment was not found.");
            db.Entry(item).Property(x => x.ConcurrencyVersion).OriginalValue = version;
            foreach (PaymentAllocation a in item.Allocations.ToList()) { obligations.Add(a.FinancialTransactionId); a.FinancialTransaction!.RemoveAllocation(a); item.RemoveAllocation(a); db.PaymentAllocations.Remove(a); }
            item.SoftDelete(stamp);
        }
        else
        {
            Receipt item = await db.Receipts.Include(x => x.Allocations).ThenInclude(x => x.FinancialTransaction)
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct) ?? throw new KeyNotFoundException("Receipt was not found.");
            db.Entry(item).Property(x => x.ConcurrencyVersion).OriginalValue = version;
            foreach (ReceiptAllocation a in item.Allocations.ToList()) { obligations.Add(a.FinancialTransactionId); a.FinancialTransaction!.RemoveAllocation(a); item.RemoveAllocation(a); db.ReceiptAllocations.Remove(a); }
            item.SoftDelete(stamp);
        }
        await SynchronizeStatuses(db, obligations, stamp, ct); await Save(db, ct); await tx.CommitAsync(ct);
    }

    public async Task<FinancialSummaryDto> SummaryAsync(long contractId, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        var totals = await db.FinancialTransactions.AsNoTracking().Where(x => x.ContractId == contractId && !x.IsDeleted)
            .GroupBy(_ => 1).Select(g => new { Claims = g.Where(x => x.TransactionTypeId == 1).Sum(x => (long?)x.AmountRial) ?? 0,
                Debts = g.Where(x => x.TransactionTypeId == 2).Sum(x => (long?)x.AmountRial) ?? 0 }).SingleOrDefaultAsync(ct);
        long paid = await db.Payments.AsNoTracking().Where(x => x.ContractId == contractId && !x.IsDeleted).SumAsync(x => (long?)x.AmountRial, ct) ?? 0;
        long received = await db.Receipts.AsNoTracking().Where(x => x.ContractId == contractId && !x.IsDeleted).SumAsync(x => (long?)x.AmountRial, ct) ?? 0;
        long claimAllocated = await db.ReceiptAllocations.AsNoTracking().Where(x => x.FinancialTransaction!.ContractId == contractId && !x.FinancialTransaction.IsDeleted && !x.Receipt!.IsDeleted).SumAsync(x => (long?)x.AmountRial, ct) ?? 0;
        long debtAllocated = await db.PaymentAllocations.AsNoTracking().Where(x => x.FinancialTransaction!.ContractId == contractId && !x.FinancialTransaction.IsDeleted && !x.Payment!.IsDeleted).SumAsync(x => (long?)x.AmountRial, ct) ?? 0;
        long claims = totals?.Claims ?? 0, debts = totals?.Debts ?? 0;
        long receivable = FinancialProjectionPolicy.Remaining(claims, claimAllocated), payable = FinancialProjectionPolicy.Remaining(debts, debtAllocated);
        string? type = await (from contract in db.Contracts.AsNoTracking() where contract.Id == contractId && !contract.IsDeleted
            join contractType in db.Set<ContractType>().AsNoTracking() on contract.ContractTypeId equals contractType.Id
            select contractType.Code).SingleOrDefaultAsync(ct);
        long? contextual = type == "SALE" ? receivable : type is "PURCHASE" or "OUTSOURCE" ? payable : null;
        return new(FinancialSummaryState.Available, claims, debts, paid, received, receivable, payable, contextual);
    }

    private async Task<SettlementRow> WriteSettlement(SettlementKind kind, SettlementUpdate? update, SettlementInput input, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        await using IDbContextTransaction tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await ValidateSettlementReferences(db, input, ct);
        HashSet<long> affected = [];
        long id;
        if (kind == SettlementKind.Payment)
        {
            Payment entity;
            if (update is null) { entity = new(input.ContractId, input.Number, input.AmountRial, input.Date, input.PaymentTypeId, input.BankId, input.ReferenceNo, input.InvoiceNo, input.ChequeNo, input.PayerDescription, input.AttachmentPath, input.Description, stamp); db.Payments.Add(entity); await Save(db, ct); }
            else { entity = await db.Payments.Include(x => x.Allocations).ThenInclude(x => x.FinancialTransaction).SingleOrDefaultAsync(x => x.Id == update.Id && !x.IsDeleted, ct) ?? throw new KeyNotFoundException("Payment was not found."); if (entity.ContractId != input.ContractId) throw new MasterDataValidationException(["Settlement contract cannot change."]); db.Entry(entity).Property(x => x.ConcurrencyVersion).OriginalValue = update.ConcurrencyVersion; foreach (PaymentAllocation a in entity.Allocations.ToList()) { affected.Add(a.FinancialTransactionId); a.FinancialTransaction!.RemoveAllocation(a); entity.RemoveAllocation(a); db.PaymentAllocations.Remove(a); } await Save(db, ct); entity.Update(input.Number, input.AmountRial, input.Date, input.PaymentTypeId, input.BankId, input.ReferenceNo, input.InvoiceNo, input.ChequeNo, input.PayerDescription, input.AttachmentPath, input.Description, stamp); }
            await AllocatePayment(db, entity, input, stamp, affected, ct); id = entity.Id;
        }
        else
        {
            Receipt entity;
            if (update is null) { entity = new(input.ContractId, input.Number, input.AmountRial, input.Date, input.PaymentTypeId, input.BankId, input.ReferenceNo, input.InvoiceNo, input.ChequeNo, input.PayerDescription, input.AttachmentPath, input.Description, stamp); db.Receipts.Add(entity); await Save(db, ct); }
            else { entity = await db.Receipts.Include(x => x.Allocations).ThenInclude(x => x.FinancialTransaction).SingleOrDefaultAsync(x => x.Id == update.Id && !x.IsDeleted, ct) ?? throw new KeyNotFoundException("Receipt was not found."); if (entity.ContractId != input.ContractId) throw new MasterDataValidationException(["Settlement contract cannot change."]); db.Entry(entity).Property(x => x.ConcurrencyVersion).OriginalValue = update.ConcurrencyVersion; foreach (ReceiptAllocation a in entity.Allocations.ToList()) { affected.Add(a.FinancialTransactionId); a.FinancialTransaction!.RemoveAllocation(a); entity.RemoveAllocation(a); db.ReceiptAllocations.Remove(a); } await Save(db, ct); entity.Update(input.Number, input.AmountRial, input.Date, input.PaymentTypeId, input.BankId, input.ReferenceNo, input.InvoiceNo, input.ChequeNo, input.PayerDescription, input.AttachmentPath, input.Description, stamp); }
            await AllocateReceipt(db, entity, input, stamp, affected, ct); id = entity.Id;
        }
        await SynchronizeStatuses(db, affected, stamp, ct); await Save(db, ct); await tx.CommitAsync(ct);
        return (await GetSettlementAsync(kind, id, ct))!;
    }

    private static async Task AllocatePayment(ContractsDbContext db, Payment payment, SettlementInput input, AuditStamp stamp, HashSet<long> affected, CancellationToken ct)
    { foreach (AllocationInput a in input.Allocations) { FinancialTransaction obligation = await db.FinancialTransactions.Include(x => x.PaymentAllocations).SingleOrDefaultAsync(x => x.Id == a.ObligationId && !x.IsDeleted, ct) ?? throw new MasterDataValidationException(["Obligation was not found."]); PaymentAllocation allocation = AllocationPolicy.Allocate(obligation, payment, a.AmountRial, stamp); db.PaymentAllocations.Add(allocation); affected.Add(obligation.Id); } }
    private static async Task AllocateReceipt(ContractsDbContext db, Receipt receipt, SettlementInput input, AuditStamp stamp, HashSet<long> affected, CancellationToken ct)
    { foreach (AllocationInput a in input.Allocations) { FinancialTransaction obligation = await db.FinancialTransactions.Include(x => x.ReceiptAllocations).SingleOrDefaultAsync(x => x.Id == a.ObligationId && !x.IsDeleted, ct) ?? throw new MasterDataValidationException(["Obligation was not found."]); ReceiptAllocation allocation = AllocationPolicy.Allocate(obligation, receipt, a.AmountRial, stamp); db.ReceiptAllocations.Add(allocation); affected.Add(obligation.Id); } }

    private static async Task SynchronizeStatuses(ContractsDbContext db, IEnumerable<long> ids, AuditStamp stamp, CancellationToken ct)
    { foreach (long id in ids.Distinct()) { FinancialTransaction item = await db.FinancialTransactions.Include(x => x.PaymentAllocations).Include(x => x.ReceiptAllocations).SingleAsync(x => x.Id == id, ct); item.SetStatus(StatusId(item.AmountRial, item.AllocatedAmountRial), stamp); } }
    private static long StatusId(long amount, long allocated) => allocated == 0 ? 1 : allocated < amount ? 2 : 3;

    private static async Task ValidateSettlementReferences(ContractsDbContext db, SettlementInput input, CancellationToken ct)
    {
        if (!await db.Contracts.AnyAsync(x => x.Id == input.ContractId && !x.IsDeleted, ct)) throw new MasterDataValidationException(["Contract is invalid."]);
        PaymentType? type = await db.Set<PaymentType>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.PaymentTypeId && x.IsActive, ct);
        if (type is null) throw new MasterDataValidationException(["Payment type is invalid."]);
        if (input.BankId.HasValue && !await db.Set<Bank>().AnyAsync(x => x.Id == input.BankId && x.IsActive, ct)) throw new MasterDataValidationException(["Bank is invalid."]);
        if (type.Code == "CHEQUE" && (string.IsNullOrWhiteSpace(input.ChequeNo) || !input.BankId.HasValue)) throw new MasterDataValidationException(["Cheque number and bank are required for cheque settlements."]);
        if (type.Code == "BANK_TRANSFER" && (!input.BankId.HasValue || string.IsNullOrWhiteSpace(input.ReferenceNo))) throw new MasterDataValidationException(["Bank and transfer reference are required for bank transfers."]);
    }
    private static async Task EnsureContractAndType(ContractsDbContext db, long contractId, long typeId, CancellationToken ct)
    { if (!await db.Contracts.AnyAsync(x => x.Id == contractId && !x.IsDeleted, ct)) throw new MasterDataValidationException(["Contract is invalid."]); if (typeId is not 1 and not 2) throw new MasterDataValidationException(["Transaction type is invalid."]); }
    private static Task<List<SimpleLookupDto>> Refs<T>(ContractsDbContext db, CancellationToken ct) where T : ReferenceEntity => db.Set<T>().AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).Select(x => new SimpleLookupDto(x.Id, x.Code, x.TitleFa, x.SortOrder)).ToListAsync(ct);

    private static IQueryable<ObligationProjection> ProjectObligations(ContractsDbContext db, IQueryable<FinancialTransaction> source, DateOnly today) =>
        from x in source
        join c in db.Contracts.AsNoTracking() on x.ContractId equals c.Id
        join ct in db.Set<ContractType>().AsNoTracking() on c.ContractTypeId equals ct.Id
        join p in db.Parties.AsNoTracking() on c.PartyId equals p.Id
        join tt in db.Set<TransactionType>().AsNoTracking() on x.TransactionTypeId equals tt.Id
        let allocated = x.TransactionTypeId == 1 ? db.ReceiptAllocations.Where(a => a.FinancialTransactionId == x.Id && !a.Receipt!.IsDeleted).Sum(a => (long?)a.AmountRial) ?? 0 : db.PaymentAllocations.Where(a => a.FinancialTransactionId == x.Id && !a.Payment!.IsDeleted).Sum(a => (long?)a.AmountRial) ?? 0
        let remaining = x.AmountRial - allocated
        join st in db.Set<FinancialTransactionStatus>().AsNoTracking() on x.StatusId equals st.Id
        select new ObligationProjection { Id=x.Id, Version=x.ConcurrencyVersion, TransactionNo=x.TransactionNo, ContractId=c.Id, ContractNo=c.ContractNo, ContractTitle=c.Title, ContractTypeTitle=ct.TitleFa, PartyName=p.Name, TypeId=x.TransactionTypeId, TypeCode=tt.Code, TypeTitle=tt.TitleFa, TransactionDate=x.TransactionDate, DueDate=x.DueDate, AmountRial=x.AmountRial, Allocated=allocated, Remaining=remaining, StatusId=x.StatusId, StatusCode=st.Code, StatusTitle=st.TitleFa, IsOverdue=x.DueDate < today && remaining > 0, DaysOverdue=x.DueDate < today && remaining > 0 ? today.DayNumber-x.DueDate.DayNumber : 0, Description=x.Description };
    private static ObligationRow ToRow(ObligationProjection x) => new(x.Id,x.Version,x.TransactionNo,x.ContractId,x.ContractNo,x.ContractTitle,x.ContractTypeTitle,x.PartyName,x.TypeId,x.TypeCode,x.TypeTitle,x.TransactionDate,x.DueDate,x.AmountRial,x.Allocated,x.Remaining,x.StatusCode,x.StatusTitle,x.IsOverdue,x.DaysOverdue,x.Description);
    private static SettlementRow ToSettlementRow(SettlementListProjection x, SettlementKind kind)=>new(x.Id,x.Version,kind,x.ContractId,x.ContractNo,x.Number,x.Date,x.Amount,x.Allocated,FinancialProjectionPolicy.Remaining(x.Amount,x.Allocated),x.PaymentType,x.Bank,x.Reference,x.InvoiceNo,x.ChequeNo,x.Payer,x.Description,x.Attachment,x.ActorId,x.Actor,x.At,[]);

    private static async Task<SettlementRow?> PaymentRow(ContractsDbContext db, long id, CancellationToken ct)
    {
        Payment? p = await db.Payments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct); if (p is null) return null;
        var head = await (from c in db.Contracts.AsNoTracking() where c.Id==p.ContractId join pt in db.Set<PaymentType>() on p.PaymentTypeId equals pt.Id join u in db.Users on p.CreatedByUserId equals u.Id join b0 in db.Set<Bank>() on p.BankId equals b0.Id into bs from b in bs.DefaultIfEmpty() select new {c.ContractNo,PaymentType=pt.TitleFa,Bank=b==null?null:b.TitleFa,User=u.DisplayName}).SingleAsync(ct);
        List<SettlementAllocationRow> allocations = await (from a in db.PaymentAllocations.AsNoTracking() where a.PaymentId==id join t in db.FinancialTransactions on a.FinancialTransactionId equals t.Id let total=db.PaymentAllocations.Where(z=>z.FinancialTransactionId==t.Id && !z.Payment!.IsDeleted).Sum(z=>(long?)z.AmountRial)??0 select new SettlementAllocationRow(t.Id,t.TransactionNo,t.DueDate,t.AmountRial,a.AmountRial,t.AmountRial-total)).ToListAsync(ct);
        long allocated=allocations.Sum(x=>x.AmountRial); return new(p.Id,p.ConcurrencyVersion,SettlementKind.Payment,p.ContractId,head.ContractNo,p.PaymentNo,p.PaymentDate,p.AmountRial,allocated,FinancialProjectionPolicy.Remaining(p.AmountRial,allocated),head.PaymentType,head.Bank,p.ReferenceNo,p.InvoiceNo,p.ChequeNo,p.PayerDescription,p.Description,p.AttachmentPath,p.CreatedByUserId!.Value,head.User,p.CreatedAtUtc,allocations);
    }
    private static async Task<SettlementRow?> ReceiptRow(ContractsDbContext db, long id, CancellationToken ct)
    {
        Receipt? r = await db.Receipts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct); if (r is null) return null;
        var head = await (from c in db.Contracts.AsNoTracking() where c.Id==r.ContractId join pt in db.Set<PaymentType>() on r.PaymentTypeId equals pt.Id join u in db.Users on r.CreatedByUserId equals u.Id join b0 in db.Set<Bank>() on r.BankId equals b0.Id into bs from b in bs.DefaultIfEmpty() select new {c.ContractNo,PaymentType=pt.TitleFa,Bank=b==null?null:b.TitleFa,User=u.DisplayName}).SingleAsync(ct);
        List<SettlementAllocationRow> allocations = await (from a in db.ReceiptAllocations.AsNoTracking() where a.ReceiptId==id join t in db.FinancialTransactions on a.FinancialTransactionId equals t.Id let total=db.ReceiptAllocations.Where(z=>z.FinancialTransactionId==t.Id && !z.Receipt!.IsDeleted).Sum(z=>(long?)z.AmountRial)??0 select new SettlementAllocationRow(t.Id,t.TransactionNo,t.DueDate,t.AmountRial,a.AmountRial,t.AmountRial-total)).ToListAsync(ct);
        long allocated=allocations.Sum(x=>x.AmountRial); return new(r.Id,r.ConcurrencyVersion,SettlementKind.Receipt,r.ContractId,head.ContractNo,r.ReceiptNo,r.ReceiptDate,r.AmountRial,allocated,FinancialProjectionPolicy.Remaining(r.AmountRial,allocated),head.PaymentType,head.Bank,r.ReferenceNo,r.InvoiceNo,r.ChequeNo,r.PayerDescription,r.Description,r.AttachmentPath,r.CreatedByUserId!.Value,head.User,r.CreatedAtUtc,allocations);
    }

    private static async Task Save(ContractsDbContext db, CancellationToken ct)
    { try { await db.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { throw new MasterDataConflictException("The financial record changed; reload and retry."); } catch (DbUpdateException e) when (e.InnerException is SqliteException s && s.SqliteErrorCode==19) { throw new MasterDataDuplicateException("A number is duplicated or an allocation constraint failed."); } }
    private sealed class ObligationProjection { public long Id{get;init;} public long Version{get;init;} public string TransactionNo{get;init;}=""; public long ContractId{get;init;} public string ContractNo{get;init;}=""; public string ContractTitle{get;init;}=""; public string ContractTypeTitle{get;init;}=""; public string PartyName{get;init;}=""; public long TypeId{get;init;} public string TypeCode{get;init;}=""; public string TypeTitle{get;init;}=""; public DateOnly TransactionDate{get;init;} public DateOnly DueDate{get;init;} public long AmountRial{get;init;} public long Allocated{get;init;} public long Remaining{get;init;} public long StatusId{get;init;} public string StatusCode{get;init;}=""; public string StatusTitle{get;init;}=""; public bool IsOverdue{get;init;} public int DaysOverdue{get;init;} public string? Description{get;init;} }
    private sealed class SettlementListProjection { public long Id{get;init;} public long Version{get;init;} public long ContractId{get;init;} public string ContractNo{get;init;}=""; public string Number{get;init;}=""; public DateOnly Date{get;init;} public long Amount{get;init;} public long Allocated{get;init;} public string PaymentType{get;init;}=""; public string? Bank{get;init;} public string? Reference{get;init;} public string? InvoiceNo{get;init;} public string? ChequeNo{get;init;} public string? Payer{get;init;} public string? Description{get;init;} public string? Attachment{get;init;} public long ActorId{get;init;} public string Actor{get;init;}=""; public DateTimeOffset At{get;init;} }
}
