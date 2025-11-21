using System;
using System.Globalization;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Integrations.GoogleDrive.Functions;

internal sealed class BackupAccountingDocumentation(
    DriveExportService exportService,
    BlobServiceClient blobServiceClient,
    ILogger<BackupAccountingDocumentation> logger)
{
    private const string ContainerName = "accounting-documentation-backups";
    private const string FileNamePrefix = "accounting-documentation-";

    [Function(nameof(BackupAccountingDocumentation))]
    public async Task RunAsync(
        [TimerTrigger("%AccountingDocumentationBackupCronSchedule%", UseMonitor = false)] TimerInfo _,
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

        const string folderId = "1k6XMCP0xjcy67bgEV6zAdxEUrdjm68Hv";
        var filesInfo = await exportService.ListFilesInFolderAsync(folderId, cancellationToken);

        logger.LogInformation("Downloading files...");

        var files = await exportService.DownloadFilesAsync(filesInfo, cancellationToken);

        logger.LogInformation("Creating zip archive...");

        var zipData = await files.ZipAsync(CompressionLevel.SmallestSize, cancellationToken);

        var dateTimeString = DateTime.UtcNow.ToString("yyyyMMddHHmmssff", CultureInfo.InvariantCulture);
        var filename = $"{FileNamePrefix}{dateTimeString}.zip";

        logger.LogInformation("Backup zip file created: {Filename}. Uploading to storage...", filename);

        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        var blobClient = containerClient.GetBlobClient(filename);
        var binaryData = BinaryData.FromBytes(zipData);
        await blobClient.UploadAsync(binaryData, cancellationToken);

        logger.LogInformation("Backup zip file '{Filename}' uploaded to storage.", filename);
    }
}
