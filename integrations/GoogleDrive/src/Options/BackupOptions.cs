using Integrations.GoogleDrive.Backups;
using Integrations.GoogleDrive.Drive;

namespace Integrations.GoogleDrive.Options;

internal sealed class BackupOptions
{
    public const string SectionName = "Backup";

    public BackupType BackupType { get; init; }

    public AccountType AccountType { get; init; }

    public string ExportFolderId { get; init; } = null!;

    public string BackupFolderId { get; init; } = null!;

    public string FileNamePrefix { get; init; } = null!;
}
