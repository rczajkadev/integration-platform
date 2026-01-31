using Integrations.Notifications;
using Microsoft.Extensions.Logging;

namespace Integrations.GoogleDrive.Backups;

internal sealed class GmailBackupNotifier(
    INotificationSender notificationSender,
    ILogger<GmailBackupNotifier> logger) : IBackupNotifier
{
    public Task NotifySuccessAsync(
        BackupType backupType,
        string filename,
        CancellationToken cancellationToken)
    {
        var subject = $"[Integration Platform] Backup {backupType} completed";
        var body = $"Backup '{backupType}' completed at {DateTimeOffset.UtcNow:O}.{Environment.NewLine}File: {filename}";
        return SendAsync(subject, body, cancellationToken);
    }

    public Task NotifyFailureAsync(
        BackupType backupType,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var subject = $"[Integration Platform] Backup {backupType} failed";
        var body = $"Backup '{backupType}' failed at {DateTimeOffset.UtcNow:O}.{Environment.NewLine}Error: {exception.GetType().Name}: {exception.Message}";
        return SendAsync(subject, body, cancellationToken);
    }

    private async Task SendAsync(string subject, string body, CancellationToken cancellationToken)
    {
        try
        {
            await notificationSender.SendAsync(subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send backup notification email.");
        }
    }
}
