using Integrations.GoogleDrive.Drive;

namespace Integrations.GoogleDrive.Options;

internal sealed class GoogleDriveOptions
{
    public const string SectionName = "GoogleDrive";

    public AccountType AccountType { get; init; }

    public string ApplicationName { get; init; } = null!;

    public string JsonCredentials { get; init; } = null!;

    public string KeyVaultSecretName { get; init; } = null!;

    public int ConcurrentDownloads { get; init; } = 1;
}
