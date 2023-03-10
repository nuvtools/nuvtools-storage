namespace NuvTools.Storage;

public class File : IFile
{
    public string Name { get; private set; }
    public string Type { get; private set; }
    public Uri Uri { get; private set; }
    public Stream Content { get; private set; }
    public string Base64String { get; private set; }

    private File(string name, string type)
    {
        Name = name;
        Type = type;
    }

    public File(string name, string type, Uri uri) : this(name, type) => Uri = uri;

    public File(string name, string type, Stream content) : this(name, type)
    {
        if (content != null) content.Position = 0;
        Content = content;
        Base64String = this.Base64StringFromContent();
    }

    public File(string name, string type, string base64String) : this(name, type)
    {
        Base64String = base64String;
        Content = this.ContentFromBase64String();
    }
}