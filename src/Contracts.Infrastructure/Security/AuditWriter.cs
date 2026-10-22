namespace Contracts.Infrastructure.Security;

public sealed class AuditWriter(IDbContextFactory<ContractsDbContext> contextFactory, ISystemClock clock) : IAuditWriter
{
    public async Task WriteAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);
        await using ContractsDbContext db = await contextFactory.CreateDbContextAsync(cancellationToken);
        db.AuditLogs.Add(new(auditEvent.UserId, auditEvent.Action, auditEvent.EntityType, auditEvent.EntityId,
            clock.UtcNow, auditEvent.CorrelationId, auditEvent.Outcome,
            AuditRedactor.Redact(auditEvent.BeforeJson), AuditRedactor.Redact(auditEvent.AfterJson)));
        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class SecurityDatabaseInitializer(IDbContextFactory<ContractsDbContext> contextFactory) : ISecurityDatabaseInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        string? databasePath = await ResolveDatabasePathAsync(cancellationToken);
        bool existed = databasePath is not null && File.Exists(databasePath);
        string? rollbackPath = existed ? databasePath + $".startup-rollback-{Guid.NewGuid():N}" : null;
        try
        {
            if (rollbackPath is not null) await CreateOnlineBackupAsync(databasePath!, rollbackPath, cancellationToken);
            await using ContractsDbContext db = await contextFactory.CreateDbContextAsync(cancellationToken);
            await db.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            if (databasePath is not null)
            {
                DeleteSidecar(databasePath + "-wal");
                DeleteSidecar(databasePath + "-shm");
                if (rollbackPath is not null && File.Exists(rollbackPath))
                {
                    if (File.Exists(databasePath)) File.Replace(rollbackPath, databasePath, null, true);
                    else File.Move(rollbackPath, databasePath);
                    rollbackPath = null;
                }
                else if (!existed && File.Exists(databasePath)) File.Delete(databasePath);
            }
            throw new InvalidOperationException("Database initialization failed. The previous database was restored; review the startup log before retrying.", exception);
        }
        finally
        {
            if (rollbackPath is not null && File.Exists(rollbackPath)) File.Delete(rollbackPath);
        }
    }

    private async Task<string?> ResolveDatabasePathAsync(CancellationToken cancellationToken)
    {
        await using ContractsDbContext db = await contextFactory.CreateDbContextAsync(cancellationToken);
        Microsoft.Data.Sqlite.SqliteConnectionStringBuilder builder = new(db.Database.GetDbConnection().ConnectionString);
        if (builder.DataSource is ":memory:" || string.IsNullOrWhiteSpace(builder.DataSource)) return null;
        return Path.GetFullPath(builder.DataSource);
    }

    private static async Task CreateOnlineBackupAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken)
    {
        Microsoft.Data.Sqlite.SqliteConnectionStringBuilder sourceBuilder = new() { DataSource = sourcePath, Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly, Pooling = false };
        Microsoft.Data.Sqlite.SqliteConnectionStringBuilder targetBuilder = new() { DataSource = destinationPath, Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate, Pooling = false };
        await using Microsoft.Data.Sqlite.SqliteConnection source = new(sourceBuilder.ConnectionString);
        await using Microsoft.Data.Sqlite.SqliteConnection target = new(targetBuilder.ConnectionString);
        await source.OpenAsync(cancellationToken);
        await target.OpenAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        source.BackupDatabase(target);
    }

    private static void DeleteSidecar(string path) { if (File.Exists(path)) File.Delete(path); }
}
