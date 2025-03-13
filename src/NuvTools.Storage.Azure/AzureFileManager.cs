using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace NuvTools.Storage.Azure;

public class AzureFileManager : IFileManager
{
    protected BlobServiceClient Credencial { get; }
    private string RepositoryName { get; }

    private readonly Lazy<BlobContainerClient> repository;

    public AzureFileManager(string connectionString, string repositoryName)
    {
        Credencial = new BlobServiceClient(connectionString);

        RepositoryName = repositoryName;

        repository = new Lazy<BlobContainerClient>(() =>
        {
            return Credencial.GetBlobContainerClient(RepositoryName);
        });
    }

    public Uri GetAccessRepositoryUri(AccessPermissions permissions = AccessPermissions.Read)
    {
        return repository.Value.GenerateSasUri(PermissionsHelper.GetPermissionsBlob(permissions), DateTime.UtcNow.AddHours(24));
    }

    public async Task<IFile?> GetFileAsync(string id, bool download = false, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));

        BlobContainerClient repo = repository.Value;
        BlobClient blob = repo.GetBlobClient(id);

        return await blob.ExistsAsync(cancellationToken).ConfigureAwait(false)
            ? await blob.ToFileAsync(download, cancellationToken).ConfigureAwait(false)
            : null;
    }

    public async Task<IReadOnlyList<IFile>> GetFilesAsync(int? pageSize, CancellationToken cancellationToken = default)
    {
        BlobContainerClient repo = repository.Value;

        if (!await repo.ExistsAsync(cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("Repository not found");

        List<IFile> files = new(pageSize ?? 0);

        await foreach (global::Azure.Page<BlobItem> page in repo.GetBlobsAsync(cancellationToken: cancellationToken).AsPages(default, pageSize))
        {
            files.AddRange(page.Values.Select(item => item.ToFile(repo.Uri)));
        }

        return files;
    }

    public async Task RemoveFileAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));

        BlobClient blob = repository.Value.GetBlobClient(id);
        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public Task<IReadOnlyList<IFile>> AddFilesAsync(IFile[] files, CancellationToken cancellationToken = default)
    {
        return AddFilesAsync(string.Empty, files, cancellationToken);
    }

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

    public async Task<IFile> AddFileAsync(IFile file, string? rootDir = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file, nameof(file));

        string fileName = FileHelper.GetFileName(file.Name);
        string fullPathFile = string.IsNullOrEmpty(rootDir) ? fileName : $"{rootDir}/{fileName}";

        BlobClient blob = repository.Value.GetBlobClient(fullPathFile);
        await blob.UploadAsync(file.Content, cancellationToken).ConfigureAwait(false);

        return await blob.ToFileAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> FileExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        BlobClient blob = repository.Value.GetBlobClient(id);
        return await blob.ExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}