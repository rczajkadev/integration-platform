using Integrations.GoogleDrive.Drive;
using Integrations.Options;

namespace Integrations.GoogleDrive.Options;

[OptionsSection("GoogleDrive")]
internal sealed class GoogleDriveOptions
{
    public AccountType AccountType { get; init; }

    public string ApplicationName { get; init; } = null!;

    public string JsonCredentials { get; init; } = null!;

    public string KeyVaultSecretName { get; init; } = null!;

    public int ConcurrentDownloads { get; init; } = 1;
}
