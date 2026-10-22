namespace Contracts.Infrastructure.ContractManagement;

using Contracts.Application.Contracts;
using Contracts.Application.MasterData;
using Contracts.Application.Presentation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class ContractStore(IDbContextFactory<ContractsDbContext> factory) : IContractStore
{
    public async Task<PageResult<ContractListItem>> SearchAsync(ContractFilter filter, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        string numberSearch = TextNormalizer.IdentifierKey(filter.Search ?? string.Empty);
        string titleSearch = TextNormalizer.SearchKey(filter.Search ?? string.Empty);
        IQueryable<Contract> contracts = db.Contracts.AsNoTracking().Where(item => !item.IsDeleted &&
            (numberSearch == "" || item.NormalizedContractNo.Contains(numberSearch) || item.NormalizedTitle.Contains(titleSearch)) &&
            (!filter.TypeId.HasValue || item.ContractTypeId == filter.TypeId) &&
            (!filter.StatusId.HasValue || item.ContractStatusId == filter.StatusId) &&
            (!filter.PartyId.HasValue || item.PartyId == filter.PartyId) &&
            (!filter.ProjectId.HasValue || item.ProjectId == filter.ProjectId) &&
            (!filter.EndDateFrom.HasValue || item.EndDate >= filter.EndDateFrom) &&
            (!filter.EndDateTo.HasValue || item.EndDate <= filter.EndDateTo));
        int total = await contracts.CountAsync(ct);
        IQueryable<ContractListProjection> query =
            from item in contracts
            join type in db.Set<ContractType>().AsNoTracking() on item.ContractTypeId equals type.Id
            join status in db.Set<ContractStatus>().AsNoTracking() on item.ContractStatusId equals status.Id
            join currency in db.Set<Currency>().AsNoTracking() on item.CurrencyId equals currency.Id
            join party in db.Parties.AsNoTracking() on item.PartyId equals party.Id
            join categoryValue in db.ContractCategories.AsNoTracking() on item.ContractCategoryId equals categoryValue.Id into categories
            from category in categories.DefaultIfEmpty()
            join projectValue in db.Projects.AsNoTracking() on item.ProjectId equals projectValue.Id into projects
            from project in projects.DefaultIfEmpty()
            let amendmentTotal = db.ContractAmendments.Where(amendment => amendment.ContractId == item.Id && !amendment.IsDeleted)
                .Sum(amendment => (long?)amendment.AmountChangeRial) ?? 0
            select new ContractListProjection
            {
                Id = item.Id, ConcurrencyVersion = item.ConcurrencyVersion, ContractNo = item.ContractNo,
                Title = item.Title, TypeTitle = type.TitleFa, CategoryTitle = category == null ? null : category.TitleFa,
                PartyName = party.Name, ProjectName = project == null ? null : project.Title, CurrencyCode = currency.Code,
                CurrencyScale = item.CurrencyScale, BaseAmountRial = item.BaseAmountRial,
                FinalAmountRial = item.BaseAmountRial + amendmentTotal, StatusTitle = status.TitleFa, EndDate = item.EndDate,
            };
        query = (filter.Sort.ToLowerInvariant(), filter.Descending) switch
        {
            ("enddate", false) => query.OrderBy(item => item.EndDate).ThenBy(item => item.ContractNo).ThenBy(item => item.Id),
            ("enddate", true) => query.OrderByDescending(item => item.EndDate).ThenByDescending(item => item.ContractNo).ThenByDescending(item => item.Id),
            ("final", false) => query.OrderBy(item => item.FinalAmountRial).ThenBy(item => item.ContractNo).ThenBy(item => item.Id),
            ("final", true) => query.OrderByDescending(item => item.FinalAmountRial).ThenByDescending(item => item.ContractNo).ThenByDescending(item => item.Id),
            ("title", false) => query.OrderBy(item => item.Title).ThenBy(item => item.ContractNo).ThenBy(item => item.Id),
            ("title", true) => query.OrderByDescending(item => item.Title).ThenByDescending(item => item.ContractNo).ThenByDescending(item => item.Id),
            (_, true) => query.OrderByDescending(item => item.ContractNo).ThenByDescending(item => item.Id),
            _ => query.OrderBy(item => item.ContractNo).ThenBy(item => item.Id),
        };
        List<ContractListItem> items = await query.Skip(filter.Skip).Take(filter.PageSize)
            .Select(item => new ContractListItem(item.Id, item.ConcurrencyVersion, item.ContractNo, item.Title,
                item.TypeTitle, item.CategoryTitle, item.PartyName, item.ProjectName, item.CurrencyCode,
                item.CurrencyScale, item.BaseAmountRial, item.FinalAmountRial, item.StatusTitle, item.EndDate)).ToListAsync(ct);
        return new(items, total, filter.Page, filter.PageSize);
    }

    public async Task<ContractDetail?> GetAsync(long id, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        return await (from item in db.Contracts.AsNoTracking() where item.Id == id && !item.IsDeleted
            join currency in db.Set<Currency>().AsNoTracking() on item.CurrencyId equals currency.Id
            join creator in db.Users.AsNoTracking() on item.CreatedByUserId equals creator.Id
            let amendmentTotal = db.ContractAmendments.Where(amendment => amendment.ContractId == item.Id && !amendment.IsDeleted)
                .Sum(amendment => (long?)amendment.AmountChangeRial) ?? 0
            let extensionDays = db.ContractAmendments.Where(amendment => amendment.ContractId == item.Id && !amendment.IsDeleted)
                .Sum(amendment => (int?)amendment.TimeExtensionDays) ?? 0
            select new ContractDetail(item.Id, item.ConcurrencyVersion, item.ContractNo, item.Title,
                item.ContractTypeId, item.ContractCategoryId, item.ProjectId, item.PartyId, item.ContractStatusId,
                item.CurrencyId, currency.Code, item.CurrencyScale, item.OriginalAmountMinor,
                item.ExchangeRateRialPerUnit, item.BaseAmountRial, item.BaseAmountRial + amendmentTotal,
                item.StartDate, item.EndDate, item.EndDate.AddDays(extensionDays), item.EndDate.DayNumber - item.StartDate.DayNumber + 1,
                item.ManagerUserId, item.Description, item.CreatedByUserId!.Value, creator.DisplayName, item.CreatedAtUtc))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<ContractReferenceData> ReferencesAsync(CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        List<SimpleLookupDto> types = await ReferenceAsync<ContractType>(db, ct);
        List<SimpleLookupDto> categories = await ReferenceAsync<ContractCategory>(db, ct);
        List<SimpleLookupDto> statuses = await ReferenceAsync<ContractStatus>(db, ct);
        List<CurrencyOption> currencies = await db.Set<Currency>().AsNoTracking().Where(item => item.IsActive)
            .OrderBy(item => item.SortOrder).Select(item => new CurrencyOption(item.Id, item.Code, item.TitleFa, item.Scale)).ToListAsync(ct);
        List<ContractOption> parties = await db.Parties.AsNoTracking().Where(item => !item.IsDeleted)
            .OrderBy(item => item.NormalizedName).Select(item => new ContractOption(item.Id, item.Name)).ToListAsync(ct);
        List<ContractOption> projects = await db.Projects.AsNoTracking().Where(item => !item.IsDeleted)
            .OrderBy(item => item.NormalizedProjectNo).Select(item => new ContractOption(item.Id, item.ProjectNo + " - " + item.Title)).ToListAsync(ct);
        return new(types, categories, statuses, currencies, parties, projects);
    }

    public async Task<ContractRelatedData> RelatedAsync(long id, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        List<AmendmentRow> amendments = await db.ContractAmendments.AsNoTracking().Where(item => item.ContractId == id && !item.IsDeleted)
            .OrderByDescending(item => item.AmendmentDate).Select(item => new AmendmentRow(item.AmendmentNo, item.AmendmentDate, item.AmountChangeRial, item.TimeExtensionDays)).ToListAsync(ct);
        List<PaymentRow> payments = await db.Payments.AsNoTracking().Where(item => item.ContractId == id && !item.IsDeleted)
            .OrderByDescending(item => item.PaymentDate).Select(item => new PaymentRow(item.PaymentDate, item.AmountRial, item.PaymentNo)).ToListAsync(ct);
        List<ReceiptRow> receipts = await db.Receipts.AsNoTracking().Where(item => item.ContractId == id && !item.IsDeleted)
            .OrderByDescending(item => item.ReceiptDate).Select(item => new ReceiptRow(item.ReceiptDate, item.AmountRial, item.ReceiptNo)).ToListAsync(ct);
        List<InvoiceRow> invoices = await db.Invoices.AsNoTracking().Where(item => item.ContractId == id && !item.IsDeleted)
            .OrderByDescending(item => item.InvoiceDate).Select(item => new InvoiceRow(item.InvoiceNo, item.InvoiceDate, item.AmountRial + item.VatAmountRial, item.DueDate)).ToListAsync(ct);
        List<GuaranteeRow> guarantees = await db.Guarantees.AsNoTracking().Where(item => item.ContractId == id && !item.IsDeleted)
            .OrderByDescending(item => item.ExpiryDate).Select(item => new GuaranteeRow(item.GuaranteeNo, item.AmountRial, item.ExpiryDate)).ToListAsync(ct);
        List<DocumentRow> documents = await db.ContractDocuments.AsNoTracking().Where(item => item.ContractId == id)
            .OrderBy(item => item.OriginalFileName).Select(item => new DocumentRow(item.OriginalFileName, item.RelativePath)).ToListAsync(ct);
        string entityId = id.ToString(CultureInfo.InvariantCulture);
        List<ContractHistoryRow> history = await (from audit in db.AuditLogs.AsNoTracking()
            where audit.EntityType == nameof(Contract) && audit.EntityId == entityId
            join actorValue in db.Users.AsNoTracking() on audit.ActorUserId equals actorValue.Id into actors
            from actor in actors.DefaultIfEmpty()
            orderby audit.OccurredAtUtc descending
            select new ContractHistoryRow(audit.OccurredAtUtc, audit.Action,
                actor == null ? "SYSTEM" : actor.DisplayName, audit.Outcome, audit.AfterJson ?? audit.BeforeJson)).ToListAsync(ct);
        return new(amendments, payments, receipts, invoices, guarantees, documents, history, FinancialSummaryDto.Unavailable);
    }

    public async Task<ContractDetail> CreateAsync(ContractInput input, int currencyScale, long effectiveRate, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        string normalizedNo = TextNormalizer.IdentifierKey(input.ContractNo);
        if (await db.Contracts.AnyAsync(item => item.NormalizedContractNo == normalizedNo, ct))
            throw new MasterDataDuplicateException("شماره قرارداد تکراری است.");
        Contract entity = new(input.ContractNo, input.Title, input.ContractTypeId, input.ContractCategoryId,
            input.ProjectId, input.PartyId, input.ContractStatusId, input.CurrencyId, input.OriginalAmountMinor,
            currencyScale, effectiveRate, input.StartDate, input.EndDate, input.ManagerUserId, input.Description, stamp);
        db.Contracts.Add(entity); await SaveAsync(db, ct); return (await GetAsync(entity.Id, ct))!;
    }

    public async Task<ContractDetail> UpdateAsync(ContractUpdate update, int currencyScale, long effectiveRate, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(ct);
        Contract entity = await db.Contracts.Include(item => item.Amendments)
            .SingleOrDefaultAsync(item => item.Id == update.Id && !item.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Contract was not found.");
        if (entity.CurrencyId != update.Values.CurrencyId && await HasFinancialDependenciesAsync(db, update.Id, ct))
            throw new MasterDataDependencyException("ارز قرارداد دارای رکورد مالی قابل تغییر نیست.");
        string normalizedNo = TextNormalizer.IdentifierKey(update.Values.ContractNo);
        if (await db.Contracts.AnyAsync(item => item.Id != update.Id && item.NormalizedContractNo == normalizedNo, ct))
            throw new MasterDataDuplicateException("شماره قرارداد تکراری است.");
        db.Entry(entity).Property(item => item.ConcurrencyVersion).OriginalValue = update.ConcurrencyVersion;
        ContractInput input = update.Values;
        entity.Update(input.ContractNo, input.Title, input.ContractTypeId, input.ContractCategoryId, input.ProjectId,
            input.PartyId, input.ContractStatusId, input.CurrencyId, input.OriginalAmountMinor, currencyScale,
            effectiveRate, input.StartDate, input.EndDate, input.ManagerUserId, input.Description, stamp);
        await SaveAsync(db, ct); await transaction.CommitAsync(ct); return (await GetAsync(entity.Id, ct))!;
    }

    public async Task DeleteAsync(long id, long version, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(ct);
        Contract entity = await db.Contracts.SingleOrDefaultAsync(item => item.Id == id && !item.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Contract was not found.");
        if (await HasAnyDependenciesAsync(db, id, ct))
            throw new MasterDataDependencyException("قرارداد دارای رکورد وابسته است و قابل حذف نیست.");
        db.Entry(entity).Property(item => item.ConcurrencyVersion).OriginalValue = version;
        entity.SoftDelete(stamp); await SaveAsync(db, ct); await transaction.CommitAsync(ct);
    }

    public async Task<bool> HasFinancialDependenciesAsync(long id, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        return await HasFinancialDependenciesAsync(db, id, ct);
    }

    public async Task<RecordNeighbor<long>> AdjacentAsync(long id, RecordDirection direction, ContractFilter filter, string stableSort, CancellationToken ct)
    {
        if (stableSort != "NormalizedContractNo,Id") throw new ArgumentException("Unsupported stable sort.", nameof(stableSort));
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        Contract? current = await db.Contracts.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id && !item.IsDeleted, ct);
        if (current is null) return new(false, default);
        string numberSearch = TextNormalizer.IdentifierKey(filter.Search ?? string.Empty);
        string titleSearch = TextNormalizer.SearchKey(filter.Search ?? string.Empty);
        Contract? adjacent = direction == RecordDirection.Next
            ? await db.Contracts.FromSqlInterpolated($"""
                SELECT * FROM Contracts WHERE IsDeleted=0
                AND ({numberSearch}='' OR instr(NormalizedContractNo,{numberSearch})>0 OR instr(NormalizedTitle,{titleSearch})>0)
                AND ({filter.TypeId} IS NULL OR ContractTypeId={filter.TypeId}) AND ({filter.StatusId} IS NULL OR ContractStatusId={filter.StatusId})
                AND ({filter.PartyId} IS NULL OR PartyId={filter.PartyId}) AND ({filter.ProjectId} IS NULL OR ProjectId={filter.ProjectId})
                AND ({filter.EndDateFrom} IS NULL OR EndDate>={filter.EndDateFrom}) AND ({filter.EndDateTo} IS NULL OR EndDate<={filter.EndDateTo})
                AND (NormalizedContractNo COLLATE BINARY>{current.NormalizedContractNo} COLLATE BINARY OR (NormalizedContractNo={current.NormalizedContractNo} AND Id>{id}))
                ORDER BY NormalizedContractNo COLLATE BINARY,Id LIMIT 1
                """).IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(ct)
            : await db.Contracts.FromSqlInterpolated($"""
                SELECT * FROM Contracts WHERE IsDeleted=0
                AND ({numberSearch}='' OR instr(NormalizedContractNo,{numberSearch})>0 OR instr(NormalizedTitle,{titleSearch})>0)
                AND ({filter.TypeId} IS NULL OR ContractTypeId={filter.TypeId}) AND ({filter.StatusId} IS NULL OR ContractStatusId={filter.StatusId})
                AND ({filter.PartyId} IS NULL OR PartyId={filter.PartyId}) AND ({filter.ProjectId} IS NULL OR ProjectId={filter.ProjectId})
                AND ({filter.EndDateFrom} IS NULL OR EndDate>={filter.EndDateFrom}) AND ({filter.EndDateTo} IS NULL OR EndDate<={filter.EndDateTo})
                AND (NormalizedContractNo COLLATE BINARY<{current.NormalizedContractNo} COLLATE BINARY OR (NormalizedContractNo={current.NormalizedContractNo} AND Id<{id}))
                ORDER BY NormalizedContractNo COLLATE BINARY DESC,Id DESC LIMIT 1
                """).IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(ct);
        return adjacent is null ? new(false, id) : new(true, adjacent.Id);
    }

    private static Task<List<SimpleLookupDto>> ReferenceAsync<T>(ContractsDbContext db, CancellationToken ct) where T : ReferenceEntity =>
        db.Set<T>().AsNoTracking().Where(item => item.IsActive).OrderBy(item => item.SortOrder)
            .Select(item => new SimpleLookupDto(item.Id, item.Code, item.TitleFa, item.SortOrder)).ToListAsync(ct);

    private static async Task<bool> HasFinancialDependenciesAsync(ContractsDbContext db, long id, CancellationToken ct) =>
        await db.ContractAmendments.AnyAsync(item => item.ContractId == id && !item.IsDeleted, ct) ||
        await db.Payments.AnyAsync(item => item.ContractId == id && !item.IsDeleted, ct) ||
        await db.Receipts.AnyAsync(item => item.ContractId == id && !item.IsDeleted, ct) ||
        await db.Invoices.AnyAsync(item => item.ContractId == id && !item.IsDeleted, ct) ||
        await db.Guarantees.AnyAsync(item => item.ContractId == id && !item.IsDeleted, ct) ||
        await db.FinancialTransactions.AnyAsync(item => item.ContractId == id && !item.IsDeleted, ct);

    private static async Task<bool> HasAnyDependenciesAsync(ContractsDbContext db, long id, CancellationToken ct) =>
        await HasFinancialDependenciesAsync(db, id, ct) ||
        await db.ContractDocuments.AnyAsync(item => item.ContractId == id, ct) ||
        await db.Alerts.AnyAsync(item => item.ContractId == id && !item.IsDeleted, ct) ||
        await db.ContractorEvaluations.AnyAsync(item => item.ContractId == id && !item.IsDeleted, ct);

    private static async Task SaveAsync(ContractsDbContext db, CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new MasterDataConflictException("قرارداد توسط کاربر دیگری تغییر کرده است؛ اطلاعات را تازه‌سازی کنید."); }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException sqlite && sqlite.SqliteErrorCode == 19)
        { throw new MasterDataDuplicateException("شماره تکراری یا وابستگی نامعتبر است."); }
    }

    private sealed class ContractListProjection
    {
        public long Id { get; init; }
        public long ConcurrencyVersion { get; init; }
        public string ContractNo { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string TypeTitle { get; init; } = string.Empty;
        public string? CategoryTitle { get; init; }
        public string PartyName { get; init; } = string.Empty;
        public string? ProjectName { get; init; }
        public string CurrencyCode { get; init; } = string.Empty;
        public int CurrencyScale { get; init; }
        public long BaseAmountRial { get; init; }
        public long FinalAmountRial { get; init; }
        public string StatusTitle { get; init; } = string.Empty;
        public DateOnly EndDate { get; init; }
    }
}
