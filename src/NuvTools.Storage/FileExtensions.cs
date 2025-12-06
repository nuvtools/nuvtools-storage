namespace NuvTools.Storage;

/// <summary>
/// Provides extension methods for converting file content between different formats.
/// </summary>
public static class FileExtensions
{
    /// <summary>
    /// Converts a Base64-encoded string to a stream.
    /// </summary>
    /// <param name="value">The file containing the Base64 string to convert.</param>
    /// <returns>
    /// A <see cref="MemoryStream"/> containing the decoded bytes, or <c>null</c> if the Base64 string is null or empty.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static Stream? ContentFromBase64String(this IFile value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrEmpty(value.Base64String)) return null;

        byte[] listaBytes = Convert.FromBase64String(value.Base64String);
        return new MemoryStream(listaBytes);
    }

    /// <summary>
    /// Converts a stream to a Base64-encoded string.
    /// </summary>
    /// <param name="value">The file containing the stream to convert.</param>
    /// <returns>
    /// A Base64-encoded string representing the stream content, or <c>null</c> if the stream is null.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string? Base64StringFromContent(this IFile value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Content is null) return null;

        byte[] inArray = new byte[(int)value.Content.Length];
        value.Content.ReadExactly(inArray, 0, (int)value.Content.Length);

        return Convert.ToBase64String(inArray);
    }
}