using Integrations.Notifications;
using Microsoft.Extensions.Logging;

namespace Integrations.GoogleDrive.Backups;

internal sealed class GmailBackupNotifier(
    INotificationSender notificationSender,
    ILogger<GmailBackupNotifier> logger) : IBackupNotifier
{
    public async Task NotifySuccessAsync(
        BackupType backupType,
        string filename,
        CancellationToken cancellationToken)
    {
        var subject = $"GoogleDrive - backup {backupType} completed";
        var body = $"""
            Backup '{backupType}' completed at {DateTimeOffset.UtcNow:O}.
            File: {filename}";
            """;

        try
        {
            await notificationSender.SendAsync(subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send backup notification email.");
        }
    }

    public async Task NotifyFailureAsync(
        BackupType backupType,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var subject = $"GoogleDrive - backup {backupType} failed";

        try
        {
            await notificationSender.SendExceptionAsync(subject, exception, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send backup failure notification email.");
        }
    }
}
