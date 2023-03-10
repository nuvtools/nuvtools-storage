namespace NuvTools.Storage;

public static class FileExtensions
{
    public static Stream ContentFromBase64String(this IFile value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));

        if (string.IsNullOrEmpty(value.Base64String)) return null;

        byte[] listaBytes = Convert.FromBase64String(value.Base64String);
        return new MemoryStream(listaBytes);
    }

    public static string Base64StringFromContent(this IFile value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));

        if (value.Content is null) return null;

        byte[] inArray = new byte[(int)value.Content.Length];
        value.Content.Read(inArray, 0, (int)value.Content.Length);
        
        return Convert.ToBase64String(inArray);
    }
}