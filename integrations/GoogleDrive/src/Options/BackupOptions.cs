using Integrations.GoogleDrive.Backups;
using Integrations.GoogleDrive.Drive;
using Integrations.Options;

namespace Integrations.GoogleDrive.Options;

[OptionsSection("Backup")]
internal sealed class BackupOptions
{
    public BackupType BackupType { get; init; }

    public AccountType AccountType { get; init; }

    public string ExportFolderId { get; init; } = null!;

    public string BackupFolderId { get; init; } = null!;

    public string FileNamePrefix { get; init; } = null!;
}
