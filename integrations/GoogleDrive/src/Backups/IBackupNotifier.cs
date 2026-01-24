namespace Integrations.GoogleDrive.Backups;

internal interface IBackupNotifier
{
    Task NotifySuccessAsync(BackupType backupType, string filename, CancellationToken cancellationToken);

    Task NotifyFailureAsync(BackupType backupType, Exception exception, CancellationToken cancellationToken);
}
