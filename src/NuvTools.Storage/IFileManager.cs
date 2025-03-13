namespace NuvTools.Storage;

public interface IFileManager
{
    Task<IFile> AddFileAsync(IFile file, string? rootDir = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IFile>> AddFilesAsync(string rootDir, IFile[] files, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IFile>> AddFilesAsync(IFile[] files, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string id, CancellationToken cancellationToken = default);
    Task RemoveFileAsync(string id, CancellationToken cancellationToken = default);
    Uri GetAccessRepositoryUri(AccessPermissions permissions = AccessPermissions.Read);
    Task<IReadOnlyList<IFile>> GetFilesAsync(int? pageSize, CancellationToken cancellationToken = default);
    Task<IFile?> GetFileAsync(string id, bool download = false, CancellationToken cancellationToken = default);
}