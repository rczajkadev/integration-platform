using System.IO;
using System.IO.Compression;

namespace Integrations.GoogleDrive.Drive;

internal static class FileExtensions
{
    public static async Task<byte[]> ZipAsync(this IEnumerable<File> files,
        CompressionLevel compressionLevel,
        CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream();

        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                var entry = zip.CreateEntry(file.Path, compressionLevel);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(file.Content.AsMemory(0, file.Content.Length), cancellationToken);
            }
        }

        return stream.ToArray();
    }
}
