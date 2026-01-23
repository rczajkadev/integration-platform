using Integrations.GoogleDrive.Backups;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.GoogleDrive.Functions;

internal sealed class BackupAccountingRecords(
    BackupHandler handler,
    ILogger<BackupAccountingRecords> logger)
{
    [Function(nameof(BackupAccountingRecords))]
    public async Task RunAsync(
        [TimerTrigger(
            "%AccountingRecordsBackupCronSchedule%",
            UseMonitor = false
#if DEBUG
            , RunOnStartup = true
#endif
        )] TimerInfo _,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Timer trigger function executed at: {DateTime}", DateTime.Now);

        try
        {
            await handler.HandleAsync(BackupType.AccountingRecords, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while creating backups.");
            throw;
        }
    }
}
