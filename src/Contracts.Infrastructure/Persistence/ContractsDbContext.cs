namespace Contracts.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore.Storage;

public sealed class ContractsDbContext : DbContext
{
    private readonly ICurrentUserService? currentUser;
    private readonly ISystemClock? clock;

    public ContractsDbContext(DbContextOptions<ContractsDbContext> options)
        : base(options) { }

    [ActivatorUtilitiesConstructor]
    public ContractsDbContext(DbContextOptions<ContractsDbContext> options, ICurrentUserService currentUser, ISystemClock clock)
        : base(options)
    {
        this.currentUser = currentUser;
        this.clock = clock;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Party> Parties => Set<Party>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectBudget> ProjectBudgets => Set<ProjectBudget>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractCategory> ContractCategories => Set<ContractCategory>();
    public DbSet<ContractAmendment> ContractAmendments => Set<ContractAmendment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Guarantee> Guarantees => Set<Guarantee>();
    public DbSet<ContractDocument> ContractDocuments => Set<ContractDocument>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<ContractorEvaluation> ContractorEvaluations => Set<ContractorEvaluation>();
    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();
    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
    public DbSet<ReceiptAllocation> ReceiptAllocations => Set<ReceiptAllocation>();
    public DbSet<OkrObjective> OkrObjectives => Set<OkrObjective>();
    public DbSet<OkrKeyResult> OkrKeyResults => Set<OkrKeyResult>();
    public DbSet<OkrMeasurement> OkrMeasurements => Set<OkrMeasurement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        CanonicalModelConfiguration.Apply(modelBuilder);
        ModelConventions.Apply(modelBuilder);
        ReferenceDataSeed.Apply(modelBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PrepareTrackedEntities();
        List<PendingAudit> audits = CapturePendingAudits();
        if (audits.Count == 0) return base.SaveChanges(acceptAllChangesOnSuccess);
        IDbContextTransaction? transaction = Database.CurrentTransaction is null ? Database.BeginTransaction() : null;
        try
        {
            int affected = base.SaveChanges(acceptAllChangesOnSuccess);
            AddPendingAudits(audits);
            affected += base.SaveChanges(acceptAllChangesOnSuccess);
            transaction?.Commit();
            return affected;
        }
        catch { transaction?.Rollback(); throw; }
        finally { transaction?.Dispose(); }
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        PrepareTrackedEntities();
        return SaveWithAuditAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private async Task<int> SaveWithAuditAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken)
    {
        List<PendingAudit> audits = CapturePendingAudits();
        if (audits.Count == 0) return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        IDbContextTransaction? transaction = Database.CurrentTransaction is null
            ? await Database.BeginTransactionAsync(cancellationToken) : null;
        try
        {
            int affected = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            AddPendingAudits(audits);
            affected += await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            return affected;
        }
        catch { if (transaction is not null) await transaction.RollbackAsync(cancellationToken); throw; }
        finally { if (transaction is not null) await transaction.DisposeAsync(); }
    }

    private void PrepareTrackedEntities()
    {
        foreach (EntityEntry<AuditLog> entry in ChangeTracker.Entries<AuditLog>())
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                throw new InvalidOperationException("Audit logs are append-only.");
            }
        }
        foreach (EntityEntry<OkrMeasurement> entry in ChangeTracker.Entries<OkrMeasurement>())
            if(entry.State is EntityState.Modified or EntityState.Deleted)throw new InvalidOperationException("OKR measurements are append-only.");

        ApplyRecorderPolicy();

        foreach (EntityEntry entry in ChangeTracker.Entries().Where(entry => entry.Entity is Entity))
        {
            PropertyEntry version = entry.Property(nameof(Entity.ConcurrencyVersion));
            if (entry.State == EntityState.Added)
            {
                version.CurrentValue = 1L;
            }
            else if (entry.State == EntityState.Modified)
            {
                version.CurrentValue = checked((long)(version.OriginalValue ?? 0L) + 1L);
            }
        }
    }

    private void ApplyRecorderPolicy()
    {
        if (currentUser is null || clock is null) return;
        foreach (EntityEntry entry in ChangeTracker.Entries().Where(item => item.Entity is AuditedEntity))
        {
            if (entry.Entity is User or Role or RolePermission) continue;
            if (entry.State is not (EntityState.Added or EntityState.Modified)) continue;
            AuthenticatedPrincipal principal = currentUser.Current
                ?? throw new UnauthorizedAccessException("Unauthenticated product writes are blocked.");
            DateTimeOffset now = clock.UtcNow;
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(AuditedEntity.CreatedAtUtc)).CurrentValue = now;
                entry.Property(nameof(AuditedEntity.CreatedByUserId)).CurrentValue = principal.UserId;
            }
            else
            {
                PropertyEntry createdAt = entry.Property(nameof(AuditedEntity.CreatedAtUtc));
                PropertyEntry createdBy = entry.Property(nameof(AuditedEntity.CreatedByUserId));
                createdAt.CurrentValue = createdAt.OriginalValue;
                createdBy.CurrentValue = createdBy.OriginalValue;
                createdAt.IsModified = false;
                createdBy.IsModified = false;
            }
            entry.Property(nameof(AuditedEntity.UpdatedAtUtc)).CurrentValue = now;
            entry.Property(nameof(AuditedEntity.UpdatedByUserId)).CurrentValue = principal.UserId;
            if (entry.Entity is SoftDeletableEntity && entry.Property(nameof(SoftDeletableEntity.IsDeleted)).CurrentValue is true)
            {
                entry.Property(nameof(SoftDeletableEntity.DeletedAtUtc)).CurrentValue = now;
                entry.Property(nameof(SoftDeletableEntity.DeletedByUserId)).CurrentValue = principal.UserId;
            }
        }
    }

    private List<PendingAudit> CapturePendingAudits()
    {
        AuthenticatedPrincipal? principal = currentUser?.Current;
        if (principal is null || clock is null) return [];
        return ChangeTracker.Entries()
            .Where(entry => entry.Entity is AuditedEntity && entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry =>
            {
                string action = entry.State == EntityState.Added ? "INSERT" :
                    entry.State == EntityState.Deleted || entry.Entity is SoftDeletableEntity && entry.Property(nameof(SoftDeletableEntity.IsDeleted)).CurrentValue is true
                        ? "DELETE" : "UPDATE";
                string[] fields = entry.Properties.Where(property => entry.State != EntityState.Modified || property.IsModified)
                    .Select(property => property.Metadata.Name).Where(name => !IsSensitiveAuditField(name)).Order(StringComparer.Ordinal).ToArray();
                string json = System.Text.Json.JsonSerializer.Serialize(new { fields });
                return new PendingAudit((Entity)entry.Entity, action, entry.Metadata.ClrType.Name, json, Guid.NewGuid().ToString("N"), principal.UserId, clock.UtcNow);
            }).ToList();
    }

    private void AddPendingAudits(IEnumerable<PendingAudit> audits)
    {
        foreach (PendingAudit item in audits)
        {
            string id = item.Entity.Id.ToString(CultureInfo.InvariantCulture);
            string? before = item.Action == "INSERT" ? null : item.FieldsJson;
            string? after = item.Action == "DELETE" ? null : item.FieldsJson;
            AuditLogs.Add(new(item.ActorUserId, item.Action, item.EntityType, id, item.AtUtc, item.CorrelationId, "SUCCESS", before, after));
        }
    }

    private static bool IsSensitiveAuditField(string name) =>
        name.Contains("Password", StringComparison.OrdinalIgnoreCase) || name.Contains("Salt", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("Token", StringComparison.OrdinalIgnoreCase) || name.Contains("Secret", StringComparison.OrdinalIgnoreCase);

    private sealed record PendingAudit(Entity Entity, string Action, string EntityType, string FieldsJson,
        string CorrelationId, long ActorUserId, DateTimeOffset AtUtc);
}
