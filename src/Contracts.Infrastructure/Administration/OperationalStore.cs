namespace Contracts.Infrastructure.Administration;

using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Contracts.Application.Administration;
using Microsoft.Data.Sqlite;

public interface IRestoreFaultInjector{public void AfterDatabaseSwitch();}
public sealed class NoRestoreFaultInjector:IRestoreFaultInjector{public void AfterDatabaseSwitch(){}}

public sealed class OperationalStore(ApplicationPaths paths,IDbContextFactory<ContractsDbContext> factory,ISystemClock clock,IRestoreFaultInjector faults):IOperationalStore,IDisposable
{
    private const int ManifestVersion=1;private static readonly JsonSerializerOptions JsonOptions=new(JsonSerializerDefaults.Web){WriteIndented=true};
    private readonly SemaphoreSlim maintenance=new(1,1);
    public async Task<SystemSettings> LoadSettingsAsync(CancellationToken ct)
    {
        if(!File.Exists(paths.ConfigurationPath))return Defaults();await using FileStream input=File.OpenRead(paths.ConfigurationPath);
        SystemSettings result=await JsonSerializer.DeserializeAsync<SystemSettings>(input,JsonOptions,ct)??throw new InvalidDataException("Settings file is empty.");result.Validate();ValidateBackup(result);return result;
    }
    public async Task SaveSettingsAsync(SystemSettings settings,CancellationToken ct)
    {
        settings.Validate();ValidateBackup(settings);Directory.CreateDirectory(Path.GetDirectoryName(paths.ConfigurationPath)!);string temp=paths.ConfigurationPath+$".{Guid.NewGuid():N}.tmp";
        try{await using(FileStream output=new(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None)){await JsonSerializer.SerializeAsync(output,settings,JsonOptions,ct);await output.FlushAsync(ct);output.Flush(true);}File.Move(temp,paths.ConfigurationPath,true);}
        finally{if(File.Exists(temp))File.Delete(temp);}
    }
    public async Task<BackupResult> BackupAsync(SystemSettings settings,CancellationToken ct)
    {await maintenance.WaitAsync(ct);try{return await BackupCoreAsync(settings,ct);}finally{maintenance.Release();}}
    private async Task<BackupResult> BackupCoreAsync(SystemSettings settings,CancellationToken ct)
    {
        settings.Validate();string backupRoot=ValidateBackup(settings);if(!File.Exists(paths.DatabasePath))throw new FileNotFoundException("The application database does not exist.",paths.DatabasePath);
        ApplicationPathPolicy.EnsurePrivateDirectory(backupRoot);long documentBytes=Directory.Exists(paths.DocumentsPath)?Directory.EnumerateFiles(paths.DocumentsPath,"*",SearchOption.AllDirectories).Sum(x=>new FileInfo(x).Length):0;
        long required=checked(new FileInfo(paths.DatabasePath).Length+documentBytes+16L*1024*1024);if(new DriveInfo(Path.GetPathRoot(backupRoot)!).AvailableFreeSpace<required)throw new IOException("Insufficient free space for backup staging.");
        DateTimeOffset now=clock.UtcNow;string name=$"ContractsNext-{now:yyyyMMdd-HHmmssfff}Z-{Guid.NewGuid():N}.cnbak",final=Path.Combine(backupRoot,name),temp=final+$".{Guid.NewGuid():N}.tmp";
        string stage=Path.Combine(backupRoot,$".build-{Guid.NewGuid():N}");Directory.CreateDirectory(stage);
        try
        {
            string dbCopy=Path.Combine(stage,"database.sqlite");await OnlineBackupAsync(dbCopy,ct);string schema=await SchemaVersionAsync(dbCopy,ct);
            string docs=Path.Combine(stage,"documents");CopyDirectory(paths.DocumentsPath,docs,ct);string config=Path.Combine(stage,"config","settings.json");Directory.CreateDirectory(Path.GetDirectoryName(config)!);await File.WriteAllTextAsync(config,JsonSerializer.Serialize(settings,JsonOptions),ct);
            List<ManifestEntry> entries=Directory.EnumerateFiles(stage,"*",SearchOption.AllDirectories).Select(file=>new ManifestEntry(Relative(stage,file),new FileInfo(file).Length,Sha256(file))).OrderBy(x=>x.Path,StringComparer.Ordinal).ToList();
            BackupManifest manifest=new(ManifestVersion,now,schema,AppVersion(),entries);await File.WriteAllTextAsync(Path.Combine(stage,"manifest.json"),JsonSerializer.Serialize(manifest,JsonOptions),ct);
            ZipFile.CreateFromDirectory(stage,temp,CompressionLevel.Optimal,false);File.Move(temp,final,false);ApplyRetention(backupRoot,settings.RetentionCount,final);
            return new(final,Sha256(final),now,new FileInfo(dbCopy).Length,documentBytes,entries.Count(x=>x.Path.StartsWith("documents/",StringComparison.Ordinal)),schema,manifest.AppVersion);
        }
        finally{if(File.Exists(temp))File.Delete(temp);if(Directory.Exists(stage))Directory.Delete(stage,true);}
    }
    public async Task<RestoreResult> RestoreAsync(string packagePath,SystemSettings settings,long actorUserId,CancellationToken ct)
    {await maintenance.WaitAsync(ct);try{return await RestoreCoreAsync(packagePath,settings,actorUserId,ct);}finally{maintenance.Release();}}
    private async Task<RestoreResult> RestoreCoreAsync(string packagePath,SystemSettings settings,long actorUserId,CancellationToken ct)
    {
        packagePath=ApplicationPathPolicy.ValidateLocalFilePath(packagePath,"restore package");if(!File.Exists(packagePath))throw new FileNotFoundException("Backup package was not found.",packagePath);
        string stage=Path.Combine(paths.RootPath,$".restore-{Guid.NewGuid():N}"),rollbackDocs=paths.DocumentsPath+$".rollback-{Guid.NewGuid():N}",rollbackDb=paths.DatabasePath+$".rollback-{Guid.NewGuid():N}",rollbackConfig=paths.ConfigurationPath+$".rollback-{Guid.NewGuid():N}";
        Directory.CreateDirectory(stage);bool dbSwitched=false,docsTouched=false,docsBacked=false,configTouched=false,configBacked=false;BackupResult? pre=null;
        try
        {
            long expanded=PackageExpandedBytes(packagePath);if(new DriveInfo(Path.GetPathRoot(paths.RootPath)!).AvailableFreeSpace<checked(expanded+new FileInfo(paths.DatabasePath).Length+16L*1024*1024))throw new IOException("Insufficient free space for restore staging.");
            ExtractSafely(packagePath,stage);BackupManifest manifest=await ReadManifestAsync(stage,ct);if(manifest.FormatVersion!=ManifestVersion)throw new InvalidDataException("Unsupported backup manifest version.");
            string[] extracted=Directory.EnumerateFiles(stage,"*",SearchOption.AllDirectories).Select(x=>Relative(stage,x)).Where(x=>x!="manifest.json").Order(StringComparer.Ordinal).ToArray();string[] declared=manifest.Files.Select(x=>x.Path).Order(StringComparer.Ordinal).ToArray();if(!extracted.SequenceEqual(declared,StringComparer.Ordinal)||declared.Distinct(StringComparer.Ordinal).Count()!=declared.Length)throw new InvalidDataException("Backup manifest does not exactly describe its payload.");
            foreach(ManifestEntry entry in manifest.Files){string file=SafeJoin(stage,entry.Path);if(!File.Exists(file)||new FileInfo(file).Length!=entry.Length||!CryptographicOperations.FixedTimeEquals(Convert.FromHexString(Sha256(file)),Convert.FromHexString(entry.Sha256)))throw new InvalidDataException($"Backup hash validation failed for '{entry.Path}'.");}
            if(!Version.TryParse(manifest.AppVersion,out Version? packageVersion)||!Version.TryParse(AppVersion(),out Version? currentVersion)||packageVersion.Major!=currentVersion.Major)throw new InvalidDataException("Backup application version is incompatible.");
            string stagedSettingsPath=SafeJoin(stage,"config/settings.json");SystemSettings stagedSettings=JsonSerializer.Deserialize<SystemSettings>(await File.ReadAllTextAsync(stagedSettingsPath,ct),JsonOptions)??throw new InvalidDataException("Backup settings are missing.");stagedSettings.Validate();stagedSettings=stagedSettings with{BackupPath=settings.BackupPath};ValidateBackup(stagedSettings);await File.WriteAllTextAsync(stagedSettingsPath,JsonSerializer.Serialize(stagedSettings,JsonOptions),ct);
            string stagedDb=SafeJoin(stage,"database.sqlite");string currentSchema=await CurrentSchemaVersionAsync(ct);string stagedSchema=await SchemaVersionAsync(stagedDb,ct);if(stagedSchema!=manifest.SchemaVersion||stagedSchema!=currentSchema)throw new InvalidDataException("Backup schema is incompatible with this application.");
            if((await IntegrityAsync(stagedDb,ct))!="ok")throw new InvalidDataException("Backup database integrity check failed.");
            if(!await IsActiveAdministratorAsync(stagedDb,actorUserId,ct))throw new InvalidDataException("The restoring administrator is not active in the selected backup.");
            pre=await BackupCoreAsync(settings,ct);
            Directory.CreateDirectory(Path.GetDirectoryName(paths.DatabasePath)!);SqliteConnection.ClearAllPools();File.Replace(stagedDb,paths.DatabasePath,rollbackDb,true);dbSwitched=true;faults.AfterDatabaseSwitch();
            string stagedDocs=SafeJoin(stage,"documents");docsTouched=true;if(Directory.Exists(paths.DocumentsPath)){Directory.Move(paths.DocumentsPath,rollbackDocs);docsBacked=true;}if(Directory.Exists(stagedDocs))Directory.Move(stagedDocs,paths.DocumentsPath);else Directory.CreateDirectory(paths.DocumentsPath);
            string stagedConfig=stagedSettingsPath;configTouched=true;if(File.Exists(paths.ConfigurationPath)){File.Move(paths.ConfigurationPath,rollbackConfig);configBacked=true;}Directory.CreateDirectory(Path.GetDirectoryName(paths.ConfigurationPath)!);File.Move(stagedConfig,paths.ConfigurationPath);
            (long contracts,long payments,long receipts)=await ReconcileAsync(paths.DatabasePath,ct);DeleteRollback(rollbackDb,rollbackDocs,rollbackConfig);return new(packagePath,pre.PackagePath,stagedSchema,contracts,payments,receipts);
        }
        catch
        {
            SqliteConnection.ClearAllPools();if(configTouched){if(File.Exists(paths.ConfigurationPath))File.Delete(paths.ConfigurationPath);if(configBacked&&File.Exists(rollbackConfig))File.Move(rollbackConfig,paths.ConfigurationPath);}
            if(docsTouched){if(Directory.Exists(paths.DocumentsPath))Directory.Delete(paths.DocumentsPath,true);if(docsBacked&&Directory.Exists(rollbackDocs))Directory.Move(rollbackDocs,paths.DocumentsPath);}
            if(dbSwitched&&File.Exists(rollbackDb))File.Replace(rollbackDb,paths.DatabasePath,null,true);throw;
        }
        finally{if(Directory.Exists(stage))Directory.Delete(stage,true);}
    }
    public async Task<HealthSnapshot> HealthAsync(SystemSettings settings,CancellationToken ct)
    {
        List<HealthCheckItem> items=[];string schema="missing";bool pending=true;
        if(!File.Exists(paths.DatabasePath))items.Add(new("DB_EXISTS","Database",HealthState.Failed,"Database file is missing.","Restore a valid backup or initialize the database."));
        else
        {schema=await CurrentSchemaVersionAsync(ct);string integrity=await IntegrityAsync(paths.DatabasePath,ct);items.Add(new("DB_INTEGRITY","SQLite integrity",integrity=="ok"?HealthState.Healthy:HealthState.Failed,integrity,integrity=="ok"?"None":"Restore a verified backup."));await using ContractsDbContext db=await factory.CreateDbContextAsync(ct);pending=(await db.Database.GetPendingMigrationsAsync(ct)).Any();items.Add(new("MIGRATIONS","Schema migrations",pending?HealthState.Failed:HealthState.Healthy,pending?"Pending migrations exist.":schema,pending?"Run the approved application upgrade.":"None"));}
        try{Directory.CreateDirectory(paths.DocumentsPath);string probe=Path.Combine(paths.DocumentsPath,$".health-{Guid.NewGuid():N}");byte[] value=RandomNumberGenerator.GetBytes(32);await File.WriteAllBytesAsync(probe,value,ct);byte[] read=await File.ReadAllBytesAsync(probe,ct);File.Delete(probe);items.Add(new("DOCUMENT_IO","Document storage",read.SequenceEqual(value)?HealthState.Healthy:HealthState.Failed,"Write/read probe completed.","None"));}catch(Exception ex){items.Add(new("DOCUMENT_IO","Document storage",HealthState.Failed,ex.GetType().Name,"Check folder availability and OS ACL."));}
        string backup=ValidateBackup(settings);DriveInfo drive=new(Path.GetPathRoot(paths.RootPath)!);HealthState disk=drive.AvailableFreeSpace<512L*1024*1024?HealthState.Warning:HealthState.Healthy;items.Add(new("FREE_DISK","Free disk",disk,$"{drive.AvailableFreeSpace/1024/1024:N0} MiB available.",disk==HealthState.Warning?"Free disk space before backup or writes.":"None"));
        bool broadAcl=ApplicationPathPolicy.HasBroadWriteAccess(paths.RootPath);items.Add(new("OS_ACL","OS access control",broadAcl?HealthState.Failed:HealthState.Healthy,broadAcl?"Broad write permission detected.":"Per-user application data is not broadly writable.",broadAcl?"Restrict ACL to the application user, SYSTEM, and local administrators.":"None"));
        string? latest=Directory.Exists(backup)?Directory.EnumerateFiles(backup,"*.cnbak").OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault():null;bool stale=latest is not null&&clock.UtcNow-File.GetLastWriteTimeUtc(latest)>TimeSpan.FromDays(settings.CriticalWarningDays);items.Add(new("LATEST_BACKUP","Latest backup",latest is null||stale?HealthState.Warning:HealthState.Healthy,latest is null?"No backup found.":$"{Path.GetFileName(latest)} @ {File.GetLastWriteTimeUtc(latest):O}",latest is null?"Create and schedule a backup.":stale?"Backup is older than the configured critical threshold.":"None"));
        return new(clock.UtcNow,AppVersion(),schema,pending,paths.MaskedRoot,items);
    }
    private SystemSettings Defaults()=>new("شرکت","گزارش سامانه قراردادها",30,30,0,7,paths.DefaultBackupPath,false,24,10);
    private string ValidateBackup(SystemSettings s)=>ApplicationPathPolicy.ValidateBackupPath(s.BackupPath,paths.DocumentsPath);
    private async Task OnlineBackupAsync(string target,CancellationToken ct){await using SqliteConnection source=new($"Data Source={paths.DatabasePath};Mode=ReadWrite;Pooling=False");await using SqliteConnection destination=new($"Data Source={target};Mode=ReadWriteCreate;Pooling=False");await source.OpenAsync(ct);await destination.OpenAsync(ct);source.BackupDatabase(destination);}
    private static async Task<string> SchemaVersionAsync(string dbPath,CancellationToken ct){await using SqliteConnection c=new($"Data Source={dbPath};Mode=ReadOnly;Pooling=False");await c.OpenAsync(ct);await using SqliteCommand cmd=c.CreateCommand();cmd.CommandText="SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 1";return (string?)await cmd.ExecuteScalarAsync(ct)??"none";}
    private async Task<string> CurrentSchemaVersionAsync(CancellationToken ct)=>await SchemaVersionAsync(paths.DatabasePath,ct);
    private static async Task<string> IntegrityAsync(string dbPath,CancellationToken ct){await using SqliteConnection c=new($"Data Source={dbPath};Mode=ReadOnly;Pooling=False");await c.OpenAsync(ct);await using SqliteCommand cmd=c.CreateCommand();cmd.CommandText="PRAGMA integrity_check";return (string?)await cmd.ExecuteScalarAsync(ct)??"unknown";}
    private static async Task<(long,long,long)> ReconcileAsync(string dbPath,CancellationToken ct){await using SqliteConnection c=new($"Data Source={dbPath};Mode=ReadOnly;Pooling=False");await c.OpenAsync(ct);async Task<long> Scalar(string sql){await using SqliteCommand cmd=c.CreateCommand();cmd.CommandText=sql;return Convert.ToInt64(await cmd.ExecuteScalarAsync(ct),CultureInfo.InvariantCulture);}return(await Scalar("SELECT COUNT(*) FROM Contracts WHERE IsDeleted=0"),await Scalar("SELECT COALESCE(SUM(AmountRial),0) FROM Payments WHERE IsDeleted=0"),await Scalar("SELECT COALESCE(SUM(AmountRial),0) FROM Receipts WHERE IsDeleted=0"));}
    private static async Task<bool> IsActiveAdministratorAsync(string dbPath,long actorUserId,CancellationToken ct){await using SqliteConnection c=new($"Data Source={dbPath};Mode=ReadOnly;Pooling=False");await c.OpenAsync(ct);await using SqliteCommand cmd=c.CreateCommand();cmd.CommandText="SELECT COUNT(*) FROM Users u JOIN Roles r ON r.Id=u.RoleId WHERE u.Id=$id AND u.IsDeleted=0 AND u.IsActive=1 AND r.Code='SYSTEM_ADMIN'";cmd.Parameters.AddWithValue("$id",actorUserId);return Convert.ToInt64(await cmd.ExecuteScalarAsync(ct),CultureInfo.InvariantCulture)==1;}
    private static void CopyDirectory(string source,string target,CancellationToken ct){if(!Directory.Exists(source))return;foreach(string file in Directory.EnumerateFiles(source,"*",SearchOption.AllDirectories)){ct.ThrowIfCancellationRequested();string relative=Path.GetRelativePath(source,file);string destination=SafeJoin(target,relative);Directory.CreateDirectory(Path.GetDirectoryName(destination)!);File.Copy(file,destination);}}
    private static void ExtractSafely(string package,string stage){using ZipArchive archive=ZipFile.OpenRead(package);foreach(ZipArchiveEntry entry in archive.Entries){string target=SafeJoin(stage,entry.FullName);if(string.IsNullOrEmpty(entry.Name)){Directory.CreateDirectory(target);continue;}Directory.CreateDirectory(Path.GetDirectoryName(target)!);entry.ExtractToFile(target,false);}}
    private static long PackageExpandedBytes(string package){using ZipArchive archive=ZipFile.OpenRead(package);long total=archive.Entries.Aggregate(0L,(sum,entry)=>checked(sum+entry.Length));if(total<=0||total>100L*1024*1024*1024)throw new InvalidDataException("Backup expanded size is invalid or exceeds 100 GiB.");return total;}
    private static string SafeJoin(string root,string relative){string normalized=relative.Replace('/',Path.DirectorySeparatorChar);string value=Path.GetFullPath(Path.Combine(root,normalized));string prefix=Path.TrimEndingDirectorySeparator(Path.GetFullPath(root))+Path.DirectorySeparatorChar;if(!value.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Package path traversal was rejected.");return value;}
    private static string Relative(string root,string file)=>Path.GetRelativePath(root,file).Replace(Path.DirectorySeparatorChar,'/');
    private static string Sha256(string file){using FileStream stream=File.OpenRead(file);return Convert.ToHexString(SHA256.HashData(stream));}
    private static string AppVersion()=>typeof(OperationalStore).Assembly.GetName().Version?.ToString()??"unknown";
    private static async Task<BackupManifest> ReadManifestAsync(string stage,CancellationToken ct)=>JsonSerializer.Deserialize<BackupManifest>(await File.ReadAllTextAsync(SafeJoin(stage,"manifest.json"),ct),JsonOptions)??throw new InvalidDataException("Backup manifest is missing.");
    private static void ApplyRetention(string root,int keep,string current){foreach(string old in Directory.EnumerateFiles(root,"ContractsNext-*.cnbak").OrderByDescending(File.GetLastWriteTimeUtc).Skip(keep).Where(x=>!string.Equals(x,current,StringComparison.OrdinalIgnoreCase)))File.Delete(old);}
    private static void DeleteRollback(string db,string docs,string config){if(File.Exists(db))File.Delete(db);if(Directory.Exists(docs))Directory.Delete(docs,true);if(File.Exists(config))File.Delete(config);}
    private sealed record ManifestEntry(string Path,long Length,string Sha256);
    private sealed record BackupManifest(int FormatVersion,DateTimeOffset CreatedAtUtc,string SchemaVersion,string AppVersion,IReadOnlyList<ManifestEntry> Files);
    public void Dispose()=>maintenance.Dispose();
}
