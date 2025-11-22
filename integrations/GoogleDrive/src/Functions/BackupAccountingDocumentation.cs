using System.Globalization;
using System.IO.Compression;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integrations.GoogleDrive.Functions;

internal sealed class BackupAccountingDocumentation(
    DriveExportService exportService,
    BlobServiceClient blobServiceClient,
    IOptions<AccountingDocumentationOptions> options,
    ILogger<BackupAccountingDocumentation> logger)
{
    [Function(nameof(BackupAccountingDocumentation))]
    public async Task RunAsync(
        [TimerTrigger(
            "%AccountingDocumentationBackupCronSchedule%",
            UseMonitor = false
#if DEBUG
            , RunOnStartup = true
#endif
            )] TimerInfo _,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Timer trigger function executed at: {DateTime}", DateTime.Now);

        try
        {
            await HandleFunctionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while creating backups.");
            throw;
        }
    }

    private async Task HandleFunctionAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Listing files in the specified folder...");

        var filesInfo = await exportService.ListFilesInFolderAsync(options.Value.DriveFolderId, cancellationToken);

        logger.LogInformation("Downloading files...");

        var files = await exportService.DownloadFilesAsync(filesInfo, cancellationToken);

        logger.LogInformation("Creating zip archive...");

        var zipData = await files.ZipAsync(CompressionLevel.SmallestSize, cancellationToken);

        var dateTimeString = DateTime.UtcNow.ToString("yyyyMMddHHmmssff", CultureInfo.InvariantCulture);
        var filename = $"{options.Value.BackupsFileNamePrefix}{dateTimeString}.zip";

        logger.LogInformation("Backup zip file created: {Filename}. Uploading to storage...", filename);

        var containerClient = blobServiceClient.GetBlobContainerClient(options.Value.BackupsContainerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        var blobClient = containerClient.GetBlobClient(filename);
        var binaryData = BinaryData.FromBytes(zipData);
        await blobClient.UploadAsync(binaryData, cancellationToken);

        logger.LogInformation("Backup zip file '{Filename}' uploaded to storage.", filename);
    }
}
