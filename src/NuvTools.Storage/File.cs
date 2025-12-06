namespace NuvTools.Storage;

/// <summary>
/// Represents a file with its metadata and content.
/// Supports three initialization modes: URI-only, stream-based, or Base64-encoded content.
/// </summary>
public class File : IFile
{
    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the MIME type of the file.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets the URI location of the file in storage.
    /// </summary>
    public Uri? Uri { get; private set; }

    /// <summary>
    /// Gets the file content as a stream.
    /// </summary>
    public Stream? Content { get; private set; }

    /// <summary>
    /// Gets the file content as a Base64-encoded string.
    /// </summary>
    public string? Base64String { get; private set; }

    private File(string name, string type)
    {
        Name = name;
        Type = type;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="File"/> class with a URI location.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <param name="type">The MIME type of the file.</param>
    /// <param name="uri">The URI location of the file in storage.</param>
    public File(string name, string type, Uri uri) : this(name, type) => Uri = uri;

    /// <summary>
    /// Initializes a new instance of the <see cref="File"/> class with stream content.
    /// The stream is automatically converted to a Base64-encoded string.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <param name="type">The MIME type of the file.</param>
    /// <param name="content">The file content as a stream.</param>
    public File(string name, string type, Stream content) : this(name, type)
    {
        if (content != null) content.Position = 0;
        Content = content;
        Base64String = this.Base64StringFromContent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="File"/> class with Base64-encoded content.
    /// The Base64 string is automatically converted to a stream.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <param name="type">The MIME type of the file.</param>
    /// <param name="base64String">The file content as a Base64-encoded string.</param>
    public File(string name, string type, string base64String) : this(name, type)
    {
        Base64String = base64String;
        Content = this.ContentFromBase64String();
    }
}