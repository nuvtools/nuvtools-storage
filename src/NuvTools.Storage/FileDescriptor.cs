namespace NuvTools.Storage;

/// <summary>
/// Describes a file in the storage repository without its content, as a listing returns it.
/// </summary>
/// <param name="Path">The full path of the file in the repository, which is also its identifier.</param>
/// <param name="Size">The size of the file, in bytes.</param>
/// <param name="LastModified">When the file was last written, if the provider reports it.</param>
/// <param name="ContentType">The MIME type of the file, if the provider reports it.</param>
public sealed record FileDescriptor(string Path, long Size, DateTimeOffset? LastModified, string? ContentType);
