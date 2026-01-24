using Integrations.Clients.Gmail;
using Integrations.GoogleDrive.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integrations.GoogleDrive.Backups;

internal sealed class GmailBackupNotifier(
    GmailClient gmailClient,
    IOptions<BackupNotificationsOptions> options,
    ILogger<GmailBackupNotifier> logger) : IBackupNotifier
{
    private readonly BackupNotificationsOptions _options = options.Value;

    public Task NotifySuccessAsync(
        BackupType backupType,
        string filename,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled) return Task.CompletedTask;

        var subject = $"[Integration Platform] Backup {backupType} completed";
        var body = $"Backup '{backupType}' completed at {DateTimeOffset.UtcNow:O}.{Environment.NewLine}File: {filename}";
        return SendAsync(subject, body, cancellationToken);
    }

    public Task NotifyFailureAsync(
        BackupType backupType,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled) return Task.CompletedTask;

        var subject = $"Backup {backupType} failed";
        var body = $"Backup '{backupType}' failed at {DateTimeOffset.UtcNow:O}.{Environment.NewLine}Error: {exception.GetType().Name}: {exception.Message}";
        return SendAsync(subject, body, cancellationToken);
    }

    private async Task SendAsync(string subject, string body, CancellationToken cancellationToken)
    {
        try
        {
            await gmailClient.SendEmailAsync(
                _options.To,
                subject,
                body,
                isHtml: false,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send backup notification email.");
        }
    }
}
