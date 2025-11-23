using Integrations.GoogleDrive.Backups;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.GoogleDrive.Functions;

internal sealed class BackupHenrySaves(
    BackupHandler handler,
    ILogger<BackupHenrySaves> logger)
{
    [Function(nameof(BackupHenrySaves))]
    public async Task RunAsync(
        [TimerTrigger(
            "%HenrySavesBackupCronSchedule%",
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
            await handler.HandleAsync(BackupType.HenrySaves, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while creating backups.");
            throw;
        }
    }
}
