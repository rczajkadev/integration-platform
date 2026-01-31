using Integrations.GoogleDrive.Options;
using Microsoft.Extensions.Options;

namespace Integrations.GoogleDrive.Backups;

internal sealed class BackupOptionsResolver(
    IOptions<List<BackupOptions>> backupOptionList,
    IOptionsSnapshot<List<GoogleDriveOptions>> googleDriveOptionList)
{
    public (BackupOptions BackupOptions, GoogleDriveOptions GoogleDriveOptions) Resolve(BackupType backupType)
    {
        var backupOptions = backupOptionList.Value.FirstOrDefault(b => b.BackupType == backupType);
        var accountType = backupOptions?.AccountType;
        var accountOptions = googleDriveOptionList.Value.FirstOrDefault(g => g.AccountType == accountType);

        return backupOptions is not null && accountOptions is not null
            ? (backupOptions, accountOptions)
            : throw new InvalidOperationException();
    }
}
