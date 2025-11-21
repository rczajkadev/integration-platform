using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace Integrations.GoogleDrive;

internal sealed class DriveExportService : IDisposable
{
    private readonly DriveService _driveService;
    private readonly SemaphoreSlim _semaphore;

    private DriveExportService(DriveService driveService, int concurrentApiCalls)
    {
        _driveService = driveService;
        _semaphore = new SemaphoreSlim(concurrentApiCalls);
    }

    public static DriveExportService Create(string jsonCredentials, int concurrentApiCalls = 100)
    {
        if (string.IsNullOrWhiteSpace(jsonCredentials))
            throw new ArgumentNullException(nameof(jsonCredentials));

        var credential = CredentialFactory.FromJson<ServiceAccountCredential>(jsonCredentials)
            .ToGoogleCredential()
            .CreateScoped(DriveService.Scope.Drive);

        var drive = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Integration Platform"
        });

        return new DriveExportService(drive, concurrentApiCalls);
    }

    public void Dispose()
    {
        _semaphore.Dispose();
        _driveService.Dispose();
    }

    public async Task<IEnumerable<FileInfo>> ListFilesInFolderAsync(
        string folderId,
        CancellationToken cancellationToken)
    {
        return await ListFilesInFolderAsync(folderId, "", cancellationToken);
    }

    public async Task<IEnumerable<File>> DownloadFilesAsync(
        IEnumerable<FileInfo> files,
        CancellationToken cancellationToken)
    {
        var tasks = files.Select(async file =>
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                await using var stream = new MemoryStream();
                var getRequest = _driveService.Files.Get(file.Id);
                await getRequest.DownloadAsync(stream, cancellationToken);
                var bytes = stream.ToArray();
                return new File(file.Path, bytes);
            }
            finally
            {
                _semaphore.Release();
            }
        }).ToArray();

        await Task.WhenAll(tasks);
        return tasks.Select(t => t.Result);
    }

    private async Task<IEnumerable<FileInfo>> ListFilesInFolderAsync(
        string folderId,
        string pathPrefix,
        CancellationToken cancellationToken)
    {
        string? nextPageToken = null;
        var files = new List<FileInfo>();

        do
        {
            var request = _driveService.Files.List();

            request.Q = $"'{folderId}' in parents and trashed = false";
            request.Spaces = "drive";
            request.Fields = "nextPageToken, files(id, name, mimeType)";
            request.PageToken = nextPageToken;

            var result = await request.ExecuteAsync(cancellationToken);

            foreach (var file in result.Files)
            {
                var fileId = file.Id;
                var filePath = pathPrefix + file.Name;
                var isFolderType = file.MimeType == "application/vnd.google-apps.folder";

                if (isFolderType)
                {
                    files.AddRange(await ListFilesInFolderAsync(file.Id, $"{filePath}/", cancellationToken));
                    continue;
                }

                files.Add(new FileInfo(fileId, filePath));
            }

            nextPageToken = result.NextPageToken;
        }
        while (!string.IsNullOrWhiteSpace(nextPageToken));

        return files;
    }
}

internal sealed record FileInfo(string Id, string Path);
