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

    public async Task<IFile?> GetFileAsync(string id, bool download = false)
    {
        var blob = repository.Value.GetBlobClient(id);

        if (await blob.ExistsAsync().ConfigureAwait(false))
            return await blob.ToFileAsync(download).ConfigureAwait(false);

        return null;
    }

    public async Task<IReadOnlyList<IFile>> GetFilesAsync(int? pageSize)
    {
        if (!await repository.Value.ExistsAsync().ConfigureAwait(false))
            await repository.Value.CreateAsync(PublicAccessType.Blob, null, null).ConfigureAwait(false);

        var fileList = new List<IFile>();

        var pages = repository.Value.GetBlobsAsync().AsPages(default, pageSize);

        await foreach (var page in pages)
        {
            foreach (var item in page.Values)
            {
                fileList.Add(item.ToFile(repository.Value.Uri));
            }
        }

        return fileList;
    }

    public async Task RemoveFileAsync(string id)
    {
        var blob = repository.Value.GetBlobClient(id);
        await blob.DeleteAsync().ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<IFile>> AddFileAsync(params IFile[] files)
    {
        return await AddFileAsync(files);
    }

    public async Task<IReadOnlyList<IFile>> AddFileAsync(string rootDir, params IFile[] files)
    {
        var resultFiles = new List<IFile>();

        foreach (var file in files)
        {
            var uploadedFile = await AddFileAsync(file, rootDir).ConfigureAwait(false);
            resultFiles.Add(uploadedFile);
        }

        return resultFiles;
    }

    private async Task<IFile> AddFileAsync(IFile file, string? rootDir = null)
    {
        var fileName = FileHelper.GetFileName(file.Name);
        var fullPathFile = fileName;

        if (rootDir != null) fullPathFile = rootDir + "/" + fileName;

        var blob = repository.Value.GetBlobClient(fullPathFile);
        await blob.UploadAsync(file.Content).ConfigureAwait(false);

        return await blob.ToFileAsync();
    }

    public async Task<bool> FileExistsAsync(string id)
    {
        var blob = repository.Value.GetBlobClient(id);
        return await blob.ExistsAsync().ConfigureAwait(false);
    }


}