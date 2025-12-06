namespace NuvTools.Storage;

/// <summary>
/// Represents a file with its metadata and content in various formats.
/// </summary>
public interface IFile
{
    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the MIME type of the file (e.g., "text/plain", "image/jpeg").
    /// </summary>
    string Type { get; }

    /// <summary>
    /// Gets the file content as a Base64-encoded string, if available.
    /// </summary>
    string? Base64String { get; }

    /// <summary>
    /// Gets the file content as a stream, if available.
    /// </summary>
    Stream? Content { get; }

    /// <summary>
    /// Gets the URI location of the file in storage, if available.
    /// </summary>
    Uri? Uri { get; }
}