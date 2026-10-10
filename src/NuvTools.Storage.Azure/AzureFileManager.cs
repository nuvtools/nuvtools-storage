using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Runtime.CompilerServices;

namespace NuvTools.Storage.Azure;

/// <summary>
/// Provides file management operations for Azure Blob Storage.
/// Implements <see cref="IFileManager"/> using Azure Blob Storage as the underlying storage provider.
/// </summary>
public class AzureFileManager : IFileManager
{
    /// <summary>
    /// Gets the Azure Blob Service client used for storage operations.
    /// </summary>
    protected BlobServiceClient Credencial { get; }

    /// <summary>
    /// Gets the name of the blob container (repository) where files are stored.
    /// </summary>
    private string RepositoryName { get; }

    private readonly Lazy<BlobContainerClient> repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureFileManager"/> class using a connection string.
    /// </summary>
    /// <param name="connectionString">The Azure Storage connection string.</param>
    /// <param name="repositoryName">The name of the blob container to use as the file repository.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionString"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="repositoryName"/> is null, empty, or whitespace.</exception>
    public AzureFileManager(string connectionString, string repositoryName)
        : this(new BlobServiceClient(connectionString), repositoryName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureFileManager"/> class using a Blob Service client.
    /// </summary>
    /// <param name="serviceClient">The Azure Blob Service client.</param>
    /// <param name="repositoryName">The name of the blob container to use as the file repository.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceClient"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="repositoryName"/> is null, empty, or whitespace.</exception>
    public AzureFileManager(BlobServiceClient serviceClient, string repositoryName)
    {
        ArgumentNullException.ThrowIfNull(serviceClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryName);

        Credencial = serviceClient;
        RepositoryName = repositoryName;

        repository = new Lazy<BlobContainerClient>(() =>
        {
            return Credencial.GetBlobContainerClient(RepositoryName);
        });
    }

    /// <summary>
    /// Gets the blob container client for the configured repository.
    /// The client is lazily initialized on first access.
    /// </summary>
    protected BlobContainerClient Repository => repository.Value;

    /// <inheritdoc />
    public Uri GetRepositorySignedUri(AccessPermissions permissions = AccessPermissions.Read)
    {
        return Repository.GenerateSasUri(PermissionsHelper.GetContainerSasPermissions(permissions), DateTime.UtcNow.AddHours(24));
    }

    /// <inheritdoc />
    public async Task<IFile?> GetFileAsync(string id, bool download = false, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));

        BlobClient blob = Repository.GetBlobClient(id);

        return await blob.ExistsAsync(cancellationToken).ConfigureAwait(false)
            ? await blob.ToFileAsync(download, cancellationToken).ConfigureAwait(false)
            : null;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when the blob container does not exist.</exception>
    public async Task<IReadOnlyList<IFile>> GetFilesAsync(int? pageSize, CancellationToken cancellationToken = default)
    {
        if (!await Repository.ExistsAsync(cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("Repository not found");

        List<IFile> files = new(pageSize ?? 0);

        await foreach (global::Azure.Page<BlobItem> page in Repository.GetBlobsAsync(cancellationToken: cancellationToken).AsPages(default, pageSize))
        {
            files.AddRange(page.Values.Select(item => item.ToFile(Repository.Uri)));
        }

        return files;
    }

    /// <inheritdoc />
    public async Task RemoveFileAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));

        BlobClient blob = Repository.GetBlobClient(id);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<IFile>> AddFilesAsync(IFile[] files, CancellationToken cancellationToken = default)
        => AddFilesAsync(string.Empty, files, cancellationToken);

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when the files array is empty.</exception>
    public async Task<IReadOnlyList<IFile>> AddFilesAsync(string rootDir, IFile[] files, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(files, nameof(files));

        if (files.Length == 0)
            throw new ArgumentException("No files to upload", nameof(files));

        if (cancellationToken.IsCancellationRequested)
            return await Task.FromCanceled<IReadOnlyList<IFile>>(cancellationToken);

        IEnumerable<Task<IFile>> uploadTasks = files.Select(file => AddFileAsync(file, rootDir, cancellationToken));

        return await Task.WhenAll(uploadTasks).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IFile> AddFileAsync(IFile file, string? rootDir = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file, nameof(file));

        string fileName = FileHelper.GetFileName(file.Name);
        string fullPathFile = string.IsNullOrEmpty(rootDir) ? fileName : $"{rootDir}/{fileName}";

        BlobClient blob = Repository.GetBlobClient(fullPathFile);
        await blob.UploadAsync(file.Content, cancellationToken).ConfigureAwait(false);

        return await blob.ToFileAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> FileExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        BlobClient blob = Repository.GetBlobClient(id);
        return await blob.ExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Uri GetFileSignedUri(string filePath, TimeSpan validFor, AccessPermissions permissions = AccessPermissions.Read)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

        BlobClient blob = Repository.GetBlobClient(filePath);
        return blob.GenerateSasUri(PermissionsHelper.GetBlobSasPermissions(permissions), DateTimeOffset.UtcNow.Add(validFor));
    }

    /// <inheritdoc />
    public async Task<IFile> AddFileAsync(Stream stream, string filePath, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

        BlobClient blob = Repository.GetBlobClient(filePath);
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return await blob.ToFileAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<FileDescriptor> ListFilesAsync(string? prefix = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? blobPrefix = string.IsNullOrEmpty(prefix) ? null : prefix;

        await foreach (BlobItem blob in Repository.GetBlobsAsync(BlobTraits.None, BlobStates.None, blobPrefix, cancellationToken).ConfigureAwait(false))
        {
            yield return new FileDescriptor(
                blob.Name,
                blob.Properties?.ContentLength ?? 0,
                blob.Properties?.LastModified,
                blob.Properties?.ContentType);
        }
    }

    /// <inheritdoc />
    public async Task<bool> DownloadFileAsync(string filePath, Stream destination, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));
        ArgumentNullException.ThrowIfNull(destination, nameof(destination));

        BlobClient blob = Repository.GetBlobClient(filePath);

        try
        {
            await blob.DownloadToAsync(destination, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (global::Azure.RequestFailedException ex) when (ex.Status == NotFound)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IFile> CopyFileAsync(string sourcePath, string targetPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath, nameof(sourcePath));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath, nameof(targetPath));

        if (string.Equals(sourcePath, targetPath, StringComparison.Ordinal))
            throw new ArgumentException("The source and the target are the same path.", nameof(targetPath));

        BlobClient source = Repository.GetBlobClient(sourcePath);
        BlobClient target = Repository.GetBlobClient(targetPath);

        try
        {
            // Read and written through this client rather than copied by the service: a service-side
            // copy needs the source to be readable by URL, which a private repository is not.
            var properties = await source.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            string contentType = properties.Value.ContentType;

            Stream content = await source.OpenReadAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            await using (content.ConfigureAwait(false))
            {
                await target.UploadAsync(content, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
                }, cancellationToken).ConfigureAwait(false);
            }

            return new File(targetPath, contentType, target.Uri);
        }
        catch (global::Azure.RequestFailedException ex) when (ex.Status == NotFound)
        {
            throw new FileNotFoundException($"The file '{sourcePath}' does not exist in the repository.", sourcePath, ex);
        }
    }

    /// <inheritdoc />
    public async Task<IFile> MoveFileAsync(string sourcePath, string targetPath, CancellationToken cancellationToken = default)
    {
        IFile moved = await CopyFileAsync(sourcePath, targetPath, cancellationToken).ConfigureAwait(false);

        // Only after the copy exists: a failure here leaves the file at both paths, never at none.
        await Repository.GetBlobClient(sourcePath)
            .DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return moved;
    }

    /// <inheritdoc />
    public async Task<int> MoveFilesAsync(string sourcePrefix, string targetPrefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePrefix, nameof(sourcePrefix));
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPrefix, nameof(targetPrefix));

        if (string.Equals(sourcePrefix, targetPrefix, StringComparison.Ordinal))
            throw new ArgumentException("The source and the target are the same prefix.", nameof(targetPrefix));

        // Listed in full before anything moves: when one prefix contains the other, a file already
        // moved would otherwise come up again in the listing.
        List<string> paths = await ListPathsAsync(sourcePrefix, cancellationToken).ConfigureAwait(false);

        foreach (string path in paths)
            await MoveFileAsync(path, targetPrefix + path[sourcePrefix.Length..], cancellationToken).ConfigureAwait(false);

        return paths.Count;
    }

    /// <inheritdoc />
    public async Task<int> RemoveFilesAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix, nameof(prefix));

        List<string> paths = await ListPathsAsync(prefix, cancellationToken).ConfigureAwait(false);

        foreach (string path in paths)
        {
            await Repository.GetBlobClient(path)
                .DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        return paths.Count;
    }

    private const int NotFound = 404;

    private async Task<List<string>> ListPathsAsync(string prefix, CancellationToken cancellationToken)
    {
        List<string> paths = [];

        await foreach (FileDescriptor file in ListFilesAsync(prefix, cancellationToken).ConfigureAwait(false))
            paths.Add(file.Path);

        return paths;
    }
}
