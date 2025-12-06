namespace NuvTools.Storage;

/// <summary>
/// Provides helper methods for file name manipulation and sanitization.
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// Sanitizes and extracts a valid file name from the provided string.
    /// Removes quotes, replaces ampersands with "and", and extracts the file name from a full path.
    /// </summary>
    /// <param name="name">The file name or path to sanitize.</param>
    /// <returns>
    /// A sanitized file name. Returns "NoName" if the input is null, empty, or whitespace.
    /// </returns>
    public static string GetFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "NoName";

        string tempName = name.Trim('"').Replace("&", "and");

        return Path.GetFileName(tempName) ?? "NoName";
    }
}
