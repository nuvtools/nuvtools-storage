namespace NuvTools.Storage;

public static class FileHelper
{
    public static string GetFileName(string name)
    {
        var tempName = !string.IsNullOrWhiteSpace(name) ? name : "NoName";

        tempName = tempName.Trim('"')
                    .Replace("&", "and");

        tempName = Path.GetFileName(tempName);

        return tempName;
    }

}
