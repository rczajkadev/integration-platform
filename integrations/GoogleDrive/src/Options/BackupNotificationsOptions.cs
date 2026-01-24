using Integrations.Options;

namespace Integrations.GoogleDrive.Options;

[OptionsSection("BackupNotifications")]
internal sealed class BackupNotificationsOptions
{
    public bool Enabled { get; init; }

    public string GmailBaseUrl { get; init; } = null!;

    public string GmailFunctionKey { get; init; } = null!;

    public string? To { get; init; }
}
