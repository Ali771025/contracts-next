namespace Contracts.Application.Administration;

using global::Contracts.Application.Security;
using global::Contracts.Application.Operations;

public sealed class SystemSettingsState
{
    private SystemSettings? current;
    public SystemSettings? Current=>Volatile.Read(ref current);
    public void Update(SystemSettings settings){settings.Validate();Interlocked.Exchange(ref current,settings);}
}

public sealed record SystemSettings(string CompanyName,string ReportHeader,int ContractWarningDays,int GuaranteeWarningDays,
    int FinancialWarningDays,int CriticalWarningDays,string BackupPath,bool AutoBackupEnabled,int AutoBackupIntervalHours,int RetentionCount)
{
    public void Validate()
    {
        if(string.IsNullOrWhiteSpace(CompanyName)||CompanyName.Trim().Length>200)throw new ArgumentException("Company name is required and limited to 200 characters.");
        if(string.IsNullOrWhiteSpace(ReportHeader)||ReportHeader.Trim().Length>300)throw new ArgumentException("Report header is required and limited to 300 characters.");
        if(ContractWarningDays is <0 or >3650||GuaranteeWarningDays is <0 or >3650||FinancialWarningDays is <0 or >3650||CriticalWarningDays is <0 or >3650)throw new ArgumentOutOfRangeException(nameof(ContractWarningDays));
        if(string.IsNullOrWhiteSpace(BackupPath))throw new ArgumentException("Backup path is required.");
        if(AutoBackupIntervalHours is <1 or >8760)throw new ArgumentOutOfRangeException(nameof(AutoBackupIntervalHours));
        if(RetentionCount is <1 or >365)throw new ArgumentOutOfRangeException(nameof(RetentionCount));
    }
}

public sealed record BackupResult(string PackagePath,string PackageSha256,DateTimeOffset CreatedAtUtc,long DatabaseBytes,long DocumentBytes,int DocumentCount,string SchemaVersion,string AppVersion);
public sealed record RestoreResult(string PackagePath,string PreRestoreBackupPath,string SchemaVersion,long ContractCount,long PaymentTotalRial,long ReceiptTotalRial);
public enum HealthState { Healthy,Warning,Failed }
public sealed record HealthCheckItem(string Code,string Title,HealthState State,string Detail,string Action);
public sealed record HealthSnapshot(DateTimeOffset CheckedAtUtc,string AppVersion,string SchemaVersion,bool HasPendingMigrations,string MaskedDataRoot,IReadOnlyList<HealthCheckItem> Items)
{public HealthState Overall=>Items.Any(x=>x.State==HealthState.Failed)?HealthState.Failed:Items.Any(x=>x.State==HealthState.Warning)?HealthState.Warning:HealthState.Healthy;}

public interface IOperationalStore
{
    public Task<SystemSettings> LoadSettingsAsync(CancellationToken ct);
    public Task SaveSettingsAsync(SystemSettings settings,CancellationToken ct);
    public Task<BackupResult> BackupAsync(SystemSettings settings,CancellationToken ct);
    public Task<RestoreResult> RestoreAsync(string packagePath,SystemSettings settings,long actorUserId,CancellationToken ct);
    public Task<HealthSnapshot> HealthAsync(SystemSettings settings,CancellationToken ct);
}

public sealed class OperationalService(IOperationalStore store,IAuthorizationService authorization,IAuditWriter audit,
    ICurrentUserService current,ICorrelationIdProvider correlation,SystemSettingsState state,AlertRuleOptions alertOptions)
{
    public async Task<SystemSettings> LoadSettingsAsync(CancellationToken ct){await authorization.RequireAsync(SecuritySection.Settings,SecurityAction.View,ct);SystemSettings settings=await store.LoadSettingsAsync(ct);Apply(settings);return settings;}
    public async Task<HealthSnapshot> HealthAsync(CancellationToken ct){await authorization.RequireAsync(SecuritySection.Settings,SecurityAction.View,ct);return await store.HealthAsync(await store.LoadSettingsAsync(ct),ct);}
    public async Task SaveSettingsAsync(SystemSettings settings,CancellationToken ct)
    {await authorization.RequireAsync(SecuritySection.Settings,SecurityAction.Admin,ct);settings.Validate();await store.SaveSettingsAsync(settings,ct);Apply(settings);await Write("SETTINGS_UPDATE","Settings","SUCCESS",ct);}
    public async Task<BackupResult> BackupAsync(CancellationToken ct)
    {await authorization.RequireAsync(SecuritySection.Settings,SecurityAction.Admin,ct);try{BackupResult result=await store.BackupAsync(await store.LoadSettingsAsync(ct),ct);await Write("BACKUP","Backup", "SUCCESS",ct);return result;}catch{await Write("BACKUP","Backup","FAILED",ct);throw;}}
    public async Task<RestoreResult> RestoreAsync(string packagePath,CancellationToken ct)
    {await authorization.RequireAsync(SecuritySection.Settings,SecurityAction.Admin,ct);try{RestoreResult result=await store.RestoreAsync(packagePath,await store.LoadSettingsAsync(ct),current.RequireAuthenticated().UserId,ct);await Write("RESTORE","Backup","SUCCESS",ct);return result;}catch{await Write("RESTORE","Backup","FAILED",ct);throw;}}
    private Task Write(string action,string type,string outcome,CancellationToken ct)=>audit.WriteAsync(new(current.RequireAuthenticated().UserId,action,type,"local",correlation.Create(),outcome),ct);
    private void Apply(SystemSettings settings){state.Update(settings);alertOptions.ContractWarningDays=settings.ContractWarningDays;alertOptions.GuaranteeWarningDays=settings.GuaranteeWarningDays;alertOptions.FinancialWarningDays=settings.FinancialWarningDays;}
}
