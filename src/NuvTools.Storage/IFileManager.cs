namespace NuvTools.Storage;

/// <summary>
/// Defines the contract for managing file storage operations.
/// </summary>
public interface IFileManager
{
    /// <summary>
    /// Adds a single file to the storage repository.
    /// </summary>
    /// <param name="file">The file to add.</param>
    /// <param name="rootDir">Optional root directory path where the file should be stored.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the added file with updated metadata.</returns>
    Task<IFile> AddFileAsync(IFile file, string? rootDir = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple files to a specified root directory in the storage repository.
    /// </summary>
    /// <param name="rootDir">The root directory path where files should be stored.</param>
    /// <param name="files">The array of files to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of added files with updated metadata.</returns>
    Task<IReadOnlyList<IFile>> AddFilesAsync(string rootDir, IFile[] files, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple files to the storage repository.
    /// </summary>
    /// <param name="files">The array of files to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of added files with updated metadata.</returns>
    Task<IReadOnlyList<IFile>> AddFilesAsync(IFile[] files, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a file exists in the storage repository.
    /// </summary>
    /// <param name="id">The unique identifier of the file.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing <c>true</c> if the file exists; otherwise, <c>false</c>.</returns>
    Task<bool> FileExistsAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a file from the storage repository.
    /// </summary>
    /// <param name="id">The unique identifier of the file to remove.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveFileAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a URI for accessing the storage repository with specified permissions.
    /// </summary>
    /// <param name="permissions">The access permissions to grant. Defaults to <see cref="AccessPermissions.Read"/>.</param>
    /// <returns>A URI that can be used to access the repository with the specified permissions.</returns>
    Uri GetAccessRepositoryUri(AccessPermissions permissions = AccessPermissions.Read);

    /// <summary>
    /// Retrieves a paginated list of files from the storage repository.
    /// </summary>
    /// <param name="pageSize">The maximum number of files to retrieve per page. If <c>null</c>, all files are retrieved.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of files.</returns>
    Task<IReadOnlyList<IFile>> GetFilesAsync(int? pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a file from the storage repository by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the file.</param>
    /// <param name="download">If <c>true</c>, downloads the file content; otherwise, returns only metadata.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the file if found; otherwise, <c>null</c>.</returns>
    Task<IFile?> GetFileAsync(string id, bool download = false, CancellationToken cancellationToken = default);
}