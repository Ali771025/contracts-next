namespace Contracts.Infrastructure.Administration;

using Contracts.Application.Administration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class AutoBackupHostedService(IOperationalStore store,IAuditWriter audit,ICorrelationIdProvider correlation,
    ILogger<AutoBackupHostedService> logger):BackgroundService
{
    private static readonly Action<ILogger,Exception?> BackupFailed=LoggerMessage.Define(LogLevel.Error,new EventId(1401,"AutoBackupFailed"),"Automatic backup failed. Review the Health screen and backup policy.");
    private static readonly Action<ILogger,Exception?> AuditFailed=LoggerMessage.Define(LogLevel.Error,new EventId(1402,"AutoBackupAuditFailed"),"Automatic backup failure audit could not be persisted.");
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer=new(TimeSpan.FromMinutes(15));
        do
        {
            try
            {
                SystemSettings settings=await store.LoadSettingsAsync(stoppingToken);if(!settings.AutoBackupEnabled)continue;
                string? latest=Directory.Exists(settings.BackupPath)?Directory.EnumerateFiles(settings.BackupPath,"*.cnbak").OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault():null;
                if(latest is not null&&DateTimeOffset.UtcNow-File.GetLastWriteTimeUtc(latest)<TimeSpan.FromHours(settings.AutoBackupIntervalHours))continue;
                await store.BackupAsync(settings,stoppingToken);await audit.WriteAsync(new(null,"AUTO_BACKUP","Backup","local",correlation.Create(),"SUCCESS",null,"{\"systemActor\":true}"),stoppingToken);
            }
            catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested){break;}
            catch(Exception ex){BackupFailed(logger,ex);try{await audit.WriteAsync(new(null,"AUTO_BACKUP","Backup","local",correlation.Create(),"FAILED",null,"{\"systemActor\":true}"),stoppingToken);}catch(Exception auditException){AuditFailed(logger,auditException);}}
        }while(await timer.WaitForNextTickAsync(stoppingToken));
    }
}
