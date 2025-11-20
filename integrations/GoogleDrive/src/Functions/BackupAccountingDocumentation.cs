using System;
using System.Globalization;
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
        logger.LogInformation("Starting backup processing...");

        const string folderId = "1k6XMCP0xjcy67bgEV6zAdxEUrdjm68Hv";
        using var stream = await exportService.ExportDirectoryAsZipAsync(folderId, cancellationToken);

        var dateTimeString = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        var filename = $"Backup-{dateTimeString}.zip";

        logger.LogInformation("Backup zip file created: {Filename}. Uploading to storage...", filename);

        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(filename);
        await blobClient.UploadAsync(stream, cancellationToken);

        logger.LogInformation("Backup zip file '{Filename}' uploaded to storage.", filename);
    }
}
