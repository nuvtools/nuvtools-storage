namespace NuvTools.Storage;

public interface IFileManager
{
    Task<IReadOnlyList<IFile>> AddFileAsync(string rootDir = null, params IFile[] files);
    Task<bool> FileExistsAsync(string id);
    Task RemoveFileAsync(string id);
    Uri GetAccessRepositoryUri(AccessPermissions permissions = AccessPermissions.Read);
    Task<IReadOnlyList<IFile>> GetFilesAsync(int? pageSize);
    Task<IFile> GetFileAsync(string id, bool download = false);
}