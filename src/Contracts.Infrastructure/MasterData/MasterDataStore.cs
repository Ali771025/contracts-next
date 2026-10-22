namespace Contracts.Infrastructure.MasterData;

using Contracts.Application.MasterData;
using Contracts.Application.Presentation;
using Microsoft.Data.Sqlite;

public sealed class MasterDataStore(IDbContextFactory<ContractsDbContext> factory) : IMasterDataStore
{
    public async Task<PageResult<PartyDto>> SearchPartiesAsync(PageRequest request, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        string search = TextNormalizer.SearchKey(request.Search ?? string.Empty);
        IQueryable<Party> source = db.Parties.AsNoTracking().Where(item => !item.IsDeleted &&
            (search == "" || item.NormalizedName.Contains(search) || (item.NationalId ?? "").Contains(search) || (item.EconomicCode ?? "").Contains(search)));
        int total = await source.CountAsync(ct);
        source = (request.Sort.ToLowerInvariant(), request.Descending) switch
        {
            ("type", false) => source.OrderBy(item => item.PartyTypeId).ThenBy(item => item.NormalizedName).ThenBy(item => item.Id),
            ("type", true) => source.OrderByDescending(item => item.PartyTypeId).ThenByDescending(item => item.NormalizedName).ThenByDescending(item => item.Id),
            (_, true) => source.OrderByDescending(item => item.NormalizedName).ThenByDescending(item => item.Id),
            _ => source.OrderBy(item => item.NormalizedName).ThenBy(item => item.Id),
        };
        List<PartyDto> items = await (from item in source.Skip(request.Skip).Take(request.PageSize)
            join type in db.Set<PartyType>().AsNoTracking() on item.PartyTypeId equals type.Id
            select new PartyDto(item.Id, item.ConcurrencyVersion, item.Name, item.PartyTypeId, type.TitleFa,
                item.NationalId, item.EconomicCode, item.Mobile, item.Phone, item.Email, item.Address, item.ContactPerson)).ToListAsync(ct);
        return new(items, total, request.Page, request.PageSize);
    }

    public async Task<PartyDto?> GetPartyAsync(long id, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        return await (from item in db.Parties.AsNoTracking() where item.Id == id && !item.IsDeleted
            join type in db.Set<PartyType>().AsNoTracking() on item.PartyTypeId equals type.Id
            select new PartyDto(item.Id, item.ConcurrencyVersion, item.Name, item.PartyTypeId, type.TitleFa,
                item.NationalId, item.EconomicCode, item.Mobile, item.Phone, item.Email, item.Address, item.ContactPerson)).SingleOrDefaultAsync(ct);
    }

    public async Task<PartyDto> CreatePartyAsync(PartyInput input, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        Party entity = new(input.Name, input.PartyTypeId, input.NationalId, input.EconomicCode, null,
            input.Mobile, input.Phone, input.Email, input.Address, input.ContactPerson, stamp);
        db.Parties.Add(entity); await SaveAsync(db, ct);
        return (await GetPartyAsync(entity.Id, ct))!;
    }

    public async Task<PartyDto> UpdatePartyAsync(PartyUpdate update, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        Party entity = await db.Parties.SingleOrDefaultAsync(item => item.Id == update.Id && !item.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Party was not found.");
        SetOriginalVersion(db, entity, update.ConcurrencyVersion);
        PartyInput input = update.Values;
        entity.Update(input.Name, input.PartyTypeId, input.NationalId, input.EconomicCode, entity.RegistrationNo,
            input.Mobile, input.Phone, input.Email, input.Address, input.ContactPerson, stamp);
        await SaveAsync(db, ct); return (await GetPartyAsync(entity.Id, ct))!;
    }

    public async Task DeletePartyAsync(long id, long version, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        Party entity = await db.Parties.SingleOrDefaultAsync(item => item.Id == id && !item.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Party was not found.");
        if (await db.Contracts.AnyAsync(item => item.PartyId == id && !item.IsDeleted, ct))
            throw new MasterDataDependencyException("طرف قرارداد دارای قرارداد وابسته است و قابل حذف نیست.");
        SetOriginalVersion(db, entity, version); entity.SoftDelete(stamp); await SaveAsync(db, ct);
    }

    public async Task<RecordNeighbor<long>> GetAdjacentPartyAsync(long id, RecordDirection direction, string search, string stableSort, CancellationToken ct)
    {
        if (!string.Equals(stableSort, "NormalizedName,Id", StringComparison.Ordinal)) throw new ArgumentException("Unsupported stable sort.", nameof(stableSort));
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        Party? current = await db.Parties.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id && !item.IsDeleted, ct);
        if (current is null) return new(false, default);
        Party? adjacent = direction == RecordDirection.Next
            ? await db.Parties.FromSqlInterpolated($"""
                SELECT * FROM Parties
                WHERE IsDeleted = 0 AND ({search} = '' OR instr(NormalizedName, {search}) > 0
                    OR instr(COALESCE(NationalId, ''), {search}) > 0 OR instr(COALESCE(EconomicCode, ''), {search}) > 0)
                  AND (NormalizedName COLLATE BINARY > {current.NormalizedName} COLLATE BINARY
                       OR (NormalizedName = {current.NormalizedName} AND Id > {id}))
                ORDER BY NormalizedName COLLATE BINARY, Id LIMIT 1
                """).IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(ct)
            : await db.Parties.FromSqlInterpolated($"""
                SELECT * FROM Parties
                WHERE IsDeleted = 0 AND ({search} = '' OR instr(NormalizedName, {search}) > 0
                    OR instr(COALESCE(NationalId, ''), {search}) > 0 OR instr(COALESCE(EconomicCode, ''), {search}) > 0)
                  AND (NormalizedName COLLATE BINARY < {current.NormalizedName} COLLATE BINARY
                       OR (NormalizedName = {current.NormalizedName} AND Id < {id}))
                ORDER BY NormalizedName COLLATE BINARY DESC, Id DESC LIMIT 1
                """).IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(ct);
        return adjacent is null ? new(false, id) : new(true, adjacent.Id);
    }

    public async Task<string?> GetPartyTypeCodeAsync(long id, CancellationToken ct)
    { await using ContractsDbContext db = await factory.CreateDbContextAsync(ct); return await db.Set<PartyType>().Where(item => item.Id == id && item.IsActive).Select(item => item.Code).SingleOrDefaultAsync(ct); }

    public async Task<PageResult<ProjectDto>> SearchProjectsAsync(PageRequest request, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        string search = TextNormalizer.SearchKey(request.Search ?? string.Empty);
        IQueryable<Project> source = db.Projects.AsNoTracking().Where(item => !item.IsDeleted &&
            (search == "" || item.NormalizedProjectNo.Contains(search) || item.NormalizedTitle.Contains(search)));
        int total = await source.CountAsync(ct);
        source = request.Descending ? source.OrderByDescending(item => item.NormalizedProjectNo).ThenByDescending(item => item.Id)
            : source.OrderBy(item => item.NormalizedProjectNo).ThenBy(item => item.Id);
        List<ProjectDto> items = await (from item in source.Skip(request.Skip).Take(request.PageSize)
            join status in db.Set<ProjectStatus>().AsNoTracking() on item.ProjectStatusId equals status.Id
            select new ProjectDto(item.Id, item.ConcurrencyVersion, item.ProjectNo, item.Title, item.Employer, item.Department,
                item.ManagerUserId, item.StartDate, item.EndDate, item.ProjectStatusId, status.TitleFa, item.Description)).ToListAsync(ct);
        return new(items, total, request.Page, request.PageSize);
    }

    public async Task<ProjectDto?> GetProjectAsync(long id, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        return await (from item in db.Projects.AsNoTracking() where item.Id == id && !item.IsDeleted
            join status in db.Set<ProjectStatus>().AsNoTracking() on item.ProjectStatusId equals status.Id
            select new ProjectDto(item.Id, item.ConcurrencyVersion, item.ProjectNo, item.Title, item.Employer, item.Department,
                item.ManagerUserId, item.StartDate, item.EndDate, item.ProjectStatusId, status.TitleFa, item.Description)).SingleOrDefaultAsync(ct);
    }

    public async Task<ProjectDto> CreateProjectAsync(ProjectInput input, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        string normalizedCode = TextNormalizer.SearchKey(input.Code);
        if (await db.Projects.AnyAsync(item => item.NormalizedProjectNo == normalizedCode, ct)) throw new MasterDataDuplicateException("کد پروژه تکراری است.");
        Project entity = new(input.Code, input.Name, input.Employer, input.Department, input.StatusId,
            input.StartDate, input.EndDate, input.ManagerUserId, input.Description, stamp);
        db.Projects.Add(entity); await SaveAsync(db, ct); return (await GetProjectAsync(entity.Id, ct))!;
    }

    public async Task<ProjectDto> UpdateProjectAsync(ProjectUpdate update, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        Project entity = await db.Projects.SingleOrDefaultAsync(item => item.Id == update.Id && !item.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Project was not found.");
        string normalizedCode = TextNormalizer.SearchKey(update.Values.Code);
        if (await db.Projects.AnyAsync(item => item.Id != update.Id && item.NormalizedProjectNo == normalizedCode, ct)) throw new MasterDataDuplicateException("کد پروژه تکراری است.");
        SetOriginalVersion(db, entity, update.ConcurrencyVersion);
        ProjectInput input = update.Values;
        entity.Update(input.Code, input.Name, input.Employer, input.Department, input.StatusId,
            input.StartDate, input.EndDate, input.ManagerUserId, input.Description, stamp);
        await SaveAsync(db, ct); return (await GetProjectAsync(entity.Id, ct))!;
    }

    public async Task DeleteProjectAsync(long id, long version, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        Project entity = await db.Projects.SingleOrDefaultAsync(item => item.Id == id && !item.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Project was not found.");
        if (await db.Contracts.AnyAsync(item => item.ProjectId == id && !item.IsDeleted, ct))
            throw new MasterDataDependencyException("پروژه دارای قرارداد وابسته است و قابل حذف نیست.");
        SetOriginalVersion(db, entity, version); entity.SoftDelete(stamp); await SaveAsync(db, ct);
    }

    public async Task<RecordNeighbor<long>> GetAdjacentProjectAsync(long id, RecordDirection direction, string search, string stableSort, CancellationToken ct)
    {
        if (!string.Equals(stableSort, "NormalizedProjectNo,Id", StringComparison.Ordinal)) throw new ArgumentException("Unsupported stable sort.", nameof(stableSort));
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        Project? current = await db.Projects.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id && !item.IsDeleted, ct);
        if (current is null) return new(false, default);
        Project? adjacent = direction == RecordDirection.Next
            ? await db.Projects.FromSqlInterpolated($"""
                SELECT * FROM Projects
                WHERE IsDeleted = 0 AND ({search} = '' OR instr(NormalizedProjectNo, {search}) > 0 OR instr(NormalizedTitle, {search}) > 0)
                  AND (NormalizedProjectNo COLLATE BINARY > {current.NormalizedProjectNo} COLLATE BINARY
                       OR (NormalizedProjectNo = {current.NormalizedProjectNo} AND Id > {id}))
                ORDER BY NormalizedProjectNo COLLATE BINARY, Id LIMIT 1
                """).IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(ct)
            : await db.Projects.FromSqlInterpolated($"""
                SELECT * FROM Projects
                WHERE IsDeleted = 0 AND ({search} = '' OR instr(NormalizedProjectNo, {search}) > 0 OR instr(NormalizedTitle, {search}) > 0)
                  AND (NormalizedProjectNo COLLATE BINARY < {current.NormalizedProjectNo} COLLATE BINARY
                       OR (NormalizedProjectNo = {current.NormalizedProjectNo} AND Id < {id}))
                ORDER BY NormalizedProjectNo COLLATE BINARY DESC, Id DESC LIMIT 1
                """).IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(ct);
        return adjacent is null ? new(false, id) : new(true, adjacent.Id);
    }

    public async Task<IReadOnlyList<ProjectBudgetDto>> GetBudgetsAsync(long projectId, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        return await (from item in db.ProjectBudgets.AsNoTracking() where item.ProjectId == projectId && !item.IsDeleted
            join currency in db.Set<Currency>().AsNoTracking() on item.CurrencyId equals currency.Id
            orderby item.FiscalYear descending, currency.SortOrder
            select new ProjectBudgetDto(item.Id, item.ConcurrencyVersion, item.ProjectId, item.FiscalYear, item.CurrencyId,
                currency.Code, item.CurrencyScale, item.ApprovedAmountMinor, item.ConsumedAmountMinor,
                item.ApprovedAmountMinor - item.ConsumedAmountMinor)).ToListAsync(ct);
    }

    public async Task<ProjectBudgetDto> CreateBudgetAsync(long projectId, ProjectBudgetInput input, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        Currency currency = await db.Set<Currency>().AsNoTracking().SingleOrDefaultAsync(item => item.Id == input.CurrencyId && item.IsActive, ct)
            ?? throw new MasterDataValidationException(["ارز فعال یافت نشد."]);
        ProjectBudget entity = new(projectId, input.FiscalYear, currency.Id, currency.Scale, input.ApprovedAmountMinor, input.ConsumedAmountMinor, stamp);
        db.ProjectBudgets.Add(entity); await SaveAsync(db, ct); return (await GetBudgetsAsync(projectId, ct)).Single(item => item.Id == entity.Id);
    }

    public async Task<ProjectBudgetDto> UpdateBudgetAsync(ProjectBudgetUpdate update, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        ProjectBudget entity = await db.ProjectBudgets.SingleOrDefaultAsync(item => item.Id == update.Id && !item.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Budget was not found.");
        Currency currency = await db.Set<Currency>().AsNoTracking().SingleOrDefaultAsync(item => item.Id == update.Values.CurrencyId && item.IsActive, ct)
            ?? throw new MasterDataValidationException(["ارز فعال یافت نشد."]);
        SetOriginalVersion(db, entity, update.ConcurrencyVersion);
        entity.Update(update.Values.FiscalYear, currency.Id, currency.Scale, update.Values.ApprovedAmountMinor, update.Values.ConsumedAmountMinor, stamp);
        await SaveAsync(db, ct); return (await GetBudgetsAsync(entity.ProjectId, ct)).Single(item => item.Id == entity.Id);
    }

    public async Task DeleteBudgetAsync(long id, long version, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        ProjectBudget entity = await db.ProjectBudgets.SingleOrDefaultAsync(item => item.Id == id && !item.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Budget was not found.");
        SetOriginalVersion(db, entity, version); entity.SoftDelete(stamp); await SaveAsync(db, ct);
    }

    public async Task<PageResult<LookupDto>> SearchLookupsAsync(AdminLookupKind kind, PageRequest request, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        return kind == AdminLookupKind.Bank
            ? await SearchReferenceAsync(db.Set<Bank>().AsNoTracking(), request, ct)
            : await SearchReferenceAsync(db.Set<PaymentType>().AsNoTracking(), request, ct);
    }

    public async Task<LookupDto> CreateLookupAsync(AdminLookupKind kind, LookupCreate input, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        return kind == AdminLookupKind.Bank
            ? await CreateReferenceAsync(db, db.Set<Bank>(), input, (id, code, title) => new Bank(id, code, title, input.SortOrder, input.IsActive, stamp), ct)
            : await CreateReferenceAsync(db, db.Set<PaymentType>(), input, (id, code, title) => new PaymentType(id, code, title, input.SortOrder, input.IsActive, stamp), ct);
    }

    public async Task<LookupDto> UpdateLookupAsync(AdminLookupKind kind, LookupUpdate update, AuditStamp stamp, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        return kind == AdminLookupKind.Bank
            ? await UpdateReferenceAsync(db, db.Set<Bank>(), update, stamp, ct)
            : await UpdateReferenceAsync(db, db.Set<PaymentType>(), update, stamp, ct);
    }

    public async Task DeactivateLookupAsync(AdminLookupKind kind, long id, long version, AuditStamp stamp, CancellationToken ct)
    {
        (string title, int sortOrder) = kind == AdminLookupKind.Bank
            ? await GetReferenceMetadataAsync<Bank>(id, ct) : await GetReferenceMetadataAsync<PaymentType>(id, ct);
        LookupUpdate update = new(id, version, title, sortOrder, false);
        await UpdateLookupAsync(kind, update, stamp, ct);
    }

    public async Task<IReadOnlyList<SimpleLookupDto>> GetActiveLookupsAsync(string kind, CancellationToken ct)
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        IQueryable<ReferenceEntity> query = kind switch
        {
            "PartyType" => db.Set<PartyType>(), "ProjectStatus" => db.Set<ProjectStatus>(), "Currency" => db.Set<Currency>(),
            "Bank" => db.Set<Bank>(), "PaymentType" => db.Set<PaymentType>(), _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        return await query.AsNoTracking().Where(item => item.IsActive).OrderBy(item => item.SortOrder).ThenBy(item => item.TitleFa)
            .Select(item => new SimpleLookupDto(item.Id, item.Code, item.TitleFa, item.SortOrder)).ToListAsync(ct);
    }

    private async Task<(string Title, int SortOrder)> GetReferenceMetadataAsync<T>(long id, CancellationToken ct) where T : ReferenceEntity
    {
        await using ContractsDbContext db = await factory.CreateDbContextAsync(ct);
        var value = await db.Set<T>().Where(item => item.Id == id)
            .Select(item => new { item.TitleFa, item.SortOrder }).SingleAsync(ct);
        return (value.TitleFa, value.SortOrder);
    }

    private static async Task<PageResult<LookupDto>> SearchReferenceAsync<T>(IQueryable<T> query, PageRequest request, CancellationToken ct) where T : ReferenceEntity
    {
        string search = TextNormalizer.SearchKey(request.Search ?? string.Empty);
        query = query.Where(item => search == "" || item.Code.Contains(search) || item.NormalizedTitleFa.Contains(search));
        int total = await query.CountAsync(ct);
        query = request.Descending ? query.OrderByDescending(item => item.SortOrder).ThenByDescending(item => item.Code)
            : query.OrderBy(item => item.SortOrder).ThenBy(item => item.Code);
        List<LookupDto> items = await query.Skip(request.Skip).Take(request.PageSize)
            .Select(item => new LookupDto(item.Id, item.ConcurrencyVersion, item.Code, item.TitleFa, item.SortOrder, item.IsActive)).ToListAsync(ct);
        return new(items, total, request.Page, request.PageSize);
    }

    private static async Task<LookupDto> CreateReferenceAsync<T>(ContractsDbContext db, DbSet<T> set, LookupCreate input,
        Func<long, string, string, T> create, CancellationToken ct) where T : ReferenceEntity
    {
        string code = input.Code.Trim().ToUpperInvariant(); string title = input.TitleFa.Trim(); string normalizedTitle = TextNormalizer.SearchKey(title);
        if (await set.AnyAsync(item => item.Code == code || item.NormalizedTitleFa == normalizedTitle, ct)) throw new MasterDataDuplicateException("کد یا عنوان lookup تکراری است.");
        long id = checked((await set.MaxAsync(item => (long?)item.Id, ct) ?? 0) + 1);
        T entity = create(id, code, title); set.Add(entity); await SaveAsync(db, ct);
        return new(entity.Id, entity.ConcurrencyVersion, entity.Code, entity.TitleFa, entity.SortOrder, entity.IsActive);
    }

    private static async Task<LookupDto> UpdateReferenceAsync<T>(ContractsDbContext db, DbSet<T> set, LookupUpdate update,
        AuditStamp stamp, CancellationToken ct) where T : ReferenceEntity
    {
        T entity = await set.SingleOrDefaultAsync(item => item.Id == update.Id, ct) ?? throw new KeyNotFoundException("Lookup was not found.");
        string normalizedTitle = TextNormalizer.SearchKey(update.TitleFa);
        if (await set.AnyAsync(item => item.Id != update.Id && item.NormalizedTitleFa == normalizedTitle, ct)) throw new MasterDataDuplicateException("عنوان lookup تکراری است.");
        SetOriginalVersion(db, entity, update.ConcurrencyVersion); entity.UpdateMetadata(update.TitleFa, update.SortOrder, update.IsActive, stamp);
        await SaveAsync(db, ct); return new(entity.Id, entity.ConcurrencyVersion, entity.Code, entity.TitleFa, entity.SortOrder, entity.IsActive);
    }

    private static void SetOriginalVersion(ContractsDbContext db, Entity entity, long version) =>
        db.Entry(entity).Property(item => item.ConcurrencyVersion).OriginalValue = version;

    private static async Task SaveAsync(ContractsDbContext db, CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException exception) { throw new MasterDataConflictException("رکورد توسط کاربر دیگری تغییر کرده است؛ اطلاعات را تازه‌سازی کنید.") { Source = exception.Source }; }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException sqlite && sqlite.SqliteErrorCode == 19)
        { throw new MasterDataDuplicateException("مقدار تکراری یا وابستگی نامعتبر است."); }
    }
}
