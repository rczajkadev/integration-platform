namespace Integrations.GoogleDrive.Drive;

internal sealed record File(string Path, byte[] Content);

internal sealed record FileInfo(string Id, string Path);
