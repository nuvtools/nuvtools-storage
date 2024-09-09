namespace NuvTools.Storage;

public interface IFile
{
    string Name { get; }
    string Type { get; }

    string? Base64String { get; }
    Stream? Content { get; }
    Uri? Uri { get; }
}