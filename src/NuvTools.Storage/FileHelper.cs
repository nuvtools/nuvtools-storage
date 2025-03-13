namespace NuvTools.Storage;

public static class FileHelper
{
    public static string GetFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "NoName";

        string tempName = name.Trim('"').Replace("&", "and");

        return Path.GetFileName(tempName) ?? "NoName";
    }
}
