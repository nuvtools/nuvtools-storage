using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NuvTools.Storage.Azure;

internal static class AzureFileManagerExtensions
{
    public static IFile ToFile(this BlobItem item, Uri baseUri)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));
        if (baseUri is null) throw new ArgumentNullException(nameof(baseUri));
        return item.ToFile(baseUri.AbsoluteUri);
    }

    public static IFile ToFile(this BlobItem item, string baseUri)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));
        if (baseUri is null) throw new ArgumentNullException(nameof(baseUri));
        return new File(item.Name, item.Properties.ContentType, new Uri($"{baseUri}/{item.Name}"));
    }

    public static async Task<IFile> ToFileAsync(this BlobClient item, bool download = false)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));

        var properties = await item.GetPropertiesAsync().ConfigureAwait(false);

        if (!download) return new File(item.Name, properties.Value.ContentType, item.Uri);

        MemoryStream memory = new();
        await item.DownloadToAsync(memory);

        return new File(item.Name, properties.Value.ContentType, memory);
    }
}
