using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace NuvTools.Storage.Azure;

/// <summary>
/// Internal extension methods for converting Azure Blob Storage objects to NuvTools <see cref="IFile"/> instances.
/// </summary>
internal static class AzureFileManagerExtensions
{
    /// <summary>
    /// Converts a <see cref="BlobItem"/> to an <see cref="IFile"/> using a base URI.
    /// </summary>
    /// <param name="item">The blob item to convert.</param>
    /// <param name="baseUri">The base URI of the blob container.</param>
    /// <returns>An <see cref="IFile"/> instance representing the blob with its URI.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> or <paramref name="baseUri"/> is null.</exception>
    public static IFile ToFile(this BlobItem item, Uri baseUri)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(baseUri);
        return item.ToFile(baseUri.AbsoluteUri);
    }

    /// <summary>
    /// Converts a <see cref="BlobItem"/> to an <see cref="IFile"/> using a base URI string.
    /// </summary>
    /// <param name="item">The blob item to convert.</param>
    /// <param name="baseUri">The base URI string of the blob container.</param>
    /// <returns>An <see cref="IFile"/> instance representing the blob with its URI.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> or <paramref name="baseUri"/> is null.</exception>
    public static IFile ToFile(this BlobItem item, string baseUri)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(baseUri);
        return new File(item.Name, item.Properties.ContentType, new Uri($"{baseUri}/{item.Name}"));
    }

    /// <summary>
    /// Converts a <see cref="BlobClient"/> to an <see cref="IFile"/>, optionally downloading the blob content.
    /// </summary>
    /// <param name="item">The blob client to convert.</param>
    /// <param name="download">If <c>true</c>, downloads the blob content into a stream; otherwise, returns only metadata with URI.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing an <see cref="IFile"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is null.</exception>
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
