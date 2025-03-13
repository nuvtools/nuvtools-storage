using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace NuvTools.Storage.Azure;

internal static class AzureFileManagerExtensions
{
    public static IFile ToFile(this BlobItem item, Uri baseUri)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(baseUri);
        return item.ToFile(baseUri.AbsoluteUri);
    }

    public static IFile ToFile(this BlobItem item, string baseUri)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(baseUri);
        return new File(item.Name, item.Properties.ContentType, new Uri($"{baseUri}/{item.Name}"));
    }

    public static async Task<IFile> ToFileAsync(this BlobClient item, bool download = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        var properties = await item.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        if (!download) return new File(item.Name, properties.Value.ContentType, item.Uri);

        MemoryStream memory = new();
        await item.DownloadToAsync(memory, cancellationToken);

        return new File(item.Name, properties.Value.ContentType, memory);
    }
}
