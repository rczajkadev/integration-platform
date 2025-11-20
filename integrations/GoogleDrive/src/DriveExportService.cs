using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using File = Google.Apis.Drive.v3.Data.File;

namespace Integrations.GoogleDrive;

internal sealed class DriveExportService(DriveService driveService)
{
    public async Task<MemoryStream> ExportDirectoryAsZipAsync(string folderId, CancellationToken cancellationToken)
    {
        var stream = new MemoryStream();

        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            await AddFolderToZipAsync(folderId, zip, "", cancellationToken);
        }

        stream.Position = 0;
        return stream;
    }

    private async Task AddFolderToZipAsync(
        string folderId,
        ZipArchive zip,
        string pathPrefix,
        CancellationToken cancellationToken)
    {
        string? nextPageToken = null;

        do
        {
            var result = await ListFilesInFolderAsync(folderId, nextPageToken, cancellationToken);

            foreach (var file in result.Files)
            {
                await HandleFileAsync(file, zip, pathPrefix, cancellationToken);
            }

            nextPageToken = result.NextPageToken;
        }
        while (!string.IsNullOrWhiteSpace(nextPageToken));
    }

    private async Task HandleFileAsync(
        File file,
        ZipArchive zip,
        string pathPrefix,
        CancellationToken cancellationToken)
    {
        var fileId = file.Id;
        var entryName = pathPrefix + file.Name;
        var isFolderType = file.MimeType == "application/vnd.google-apps.folder";

        if (isFolderType)
        {
            await AddFolderToZipAsync(fileId, zip, $"{entryName}/", cancellationToken);
            return;
        }

        await AddEntryToZipAsync(zip, entryName, fileId, cancellationToken);
    }

    private async Task AddEntryToZipAsync(
        ZipArchive zip,
        string entryName,
        string fileId,
        CancellationToken cancellationToken)
    {
        var entry = zip.CreateEntry(entryName, CompressionLevel.SmallestSize);
        await using var entryStream = entry.Open();
        var getRequest = driveService.Files.Get(fileId);
        await getRequest.DownloadAsync(entryStream, cancellationToken);
    }

    private async Task<FileList> ListFilesInFolderAsync(
        string folderId,
        string? nextPageToken,
        CancellationToken cancellationToken)
    {
        var request = driveService.Files.List();

        request.Q = $"'{folderId}' in parents and trashed = false";
        request.Spaces = "drive";
        request.Fields = "nextPageToken, files(id, name, mimeType)";
        request.PageToken = nextPageToken;

        return await request.ExecuteAsync(cancellationToken);
    }
}
