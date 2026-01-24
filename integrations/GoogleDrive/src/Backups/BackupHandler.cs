using System.Globalization;
using System.IO.Compression;
using Azure.Security.KeyVault.Secrets;
using Integrations.GoogleDrive.Drive;
using Microsoft.Extensions.Logging;

namespace Integrations.GoogleDrive.Backups;

internal sealed class BackupHandler(
    BackupOptionsResolver optionsResolver,
    SecretClient secretClient,
    ILogger<BackupHandler> logger)
{
    public async Task<string> HandleAsync(BackupType backupType, CancellationToken cancellationToken)
    {
        var (backupOptions, driveOptions) = optionsResolver.Resolve(backupType);
        using var client = await DriveClient.CreateAsync(driveOptions, secretClient, cancellationToken);

        logger.LogInformation("Listing files in folder...");

        var filesInfo = await client.ListFilesInFolderAsync(backupOptions.ExportFolderId, cancellationToken);

        logger.LogInformation("Downloading files...");

        var files = await client.DownloadFilesAsync(filesInfo, cancellationToken);

        logger.LogInformation("Creating zip archive...");

        var zipData = await files.ZipAsync(CompressionLevel.SmallestSize, cancellationToken);

        var dateTimeString = DateTime.UtcNow.ToString("yyyyMMddHHmmssff", CultureInfo.InvariantCulture);
        var filename = $"{backupOptions.FileNamePrefix}{dateTimeString}.zip";

        logger.LogInformation("Backup zip file created: {Filename}. Uploading to storage...", filename);

        const string contentType = "application/zip";
        await client.UploadFileAsync(filename, backupOptions.BackupFolderId, zipData, contentType, cancellationToken);

        logger.LogInformation("Backup zip file '{Filename}' uploaded to storage.", filename);

        return filename;
    }
}
