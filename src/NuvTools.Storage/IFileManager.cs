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
    /// Gets a signed URI for accessing the storage repository with specified permissions.
    /// </summary>
    /// <param name="permissions">The access permissions to grant. Defaults to <see cref="AccessPermissions.Read"/>.</param>
    /// <returns>A signed URI that can be used to access the repository with the specified permissions.</returns>
    Uri GetRepositorySignedUri(AccessPermissions permissions = AccessPermissions.Read);

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

    /// <summary>
    /// Gets a signed URI for accessing a specific file with specified permissions.
    /// </summary>
    /// <param name="filePath">The path to the file in the repository.</param>
    /// <param name="validFor">The duration for which the signed URI should be valid.</param>
    /// <param name="permissions">The access permissions to grant. Defaults to <see cref="AccessPermissions.Read"/>.</param>
    /// <returns>A signed URI that can be used to access the file with the specified permissions.</returns>
    Uri GetFileSignedUri(string filePath, TimeSpan validFor, AccessPermissions permissions = AccessPermissions.Read);

    /// <summary>
    /// Uploads a stream directly to storage at the specified path.
    /// </summary>
    /// <param name="stream">The stream containing the file content.</param>
    /// <param name="filePath">The full path where the file should be stored in the repository.</param>
    /// <param name="contentType">The MIME type of the file content.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the uploaded file metadata.</returns>
    Task<IFile> AddFileAsync(Stream stream, string filePath, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the files whose path starts with a prefix, without downloading them.
    /// </summary>
    /// <param name="prefix">
    /// The beginning of the paths to list, such as <c>invoices/2026/</c>. A repository has no real
    /// folders: a folder is the prefix its files share. <c>null</c> or empty lists every file.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The files found, as the provider returns them, read page by page as the sequence is consumed.</returns>
    IAsyncEnumerable<FileDescriptor> ListFilesAsync(string? prefix = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the content of a file to a stream.
    /// </summary>
    /// <param name="filePath">The path of the file in the repository.</param>
    /// <param name="destination">The stream the content is written to. It is left open.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing <c>true</c> if the file was downloaded, or <c>false</c> if it does not exist.</returns>
    Task<bool> DownloadFileAsync(string filePath, Stream destination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies a file to another path in the same repository, replacing what is there.
    /// </summary>
    /// <param name="sourcePath">The path of the file to copy.</param>
    /// <param name="targetPath">The path of the copy.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the metadata of the copy.</returns>
    /// <exception cref="FileNotFoundException">Thrown when <paramref name="sourcePath"/> does not exist.</exception>
    /// <exception cref="ArgumentException">Thrown when both paths are the same.</exception>
    Task<IFile> CopyFileAsync(string sourcePath, string targetPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a file to another path in the same repository, which is also how a file is renamed.
    /// </summary>
    /// <param name="sourcePath">The path of the file to move.</param>
    /// <param name="targetPath">The new path of the file. What is there is replaced.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the metadata of the file at its new path.</returns>
    /// <remarks>
    /// The file is copied and then the original is removed. If the removal fails the file exists at
    /// both paths; it is never lost.
    /// </remarks>
    /// <exception cref="FileNotFoundException">Thrown when <paramref name="sourcePath"/> does not exist.</exception>
    /// <exception cref="ArgumentException">Thrown when both paths are the same.</exception>
    Task<IFile> MoveFileAsync(string sourcePath, string targetPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves every file whose path starts with a prefix to another prefix, keeping the rest of each
    /// path — which is how a folder is renamed.
    /// </summary>
    /// <param name="sourcePrefix">The beginning of the paths to move, such as <c>drafts/</c>.</param>
    /// <param name="targetPrefix">What replaces <paramref name="sourcePrefix"/> in each path, such as <c>final/</c>.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of files moved.</returns>
    /// <remarks>
    /// Files are moved one at a time. If it stops midway, the files already moved stay at the target
    /// and the others at the source; running it again finishes the move.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when a prefix is empty or both are the same.</exception>
    Task<int> MoveFilesAsync(string sourcePrefix, string targetPrefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes every file whose path starts with a prefix — which is how a folder is removed.
    /// </summary>
    /// <param name="prefix">The beginning of the paths to remove. It cannot be empty: emptying a repository is not done by accident.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of files removed.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="prefix"/> is empty.</exception>
    Task<int> RemoveFilesAsync(string prefix, CancellationToken cancellationToken = default);
}