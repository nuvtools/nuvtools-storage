using NUnit.Framework;
using System;
using System.IO;

namespace NuvTools.Storage.Azure.Test;


[TestFixture]
public class FileExtensionsTests
{
    [Test]
    public void ContentFromBase64String_ShouldReturnStream_WhenValidBase64()
    {
        var base64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        var file = new MockFile { Base64String = base64 };

        var stream = file.ContentFromBase64String();

        Assert.That(stream, Is.Not.Null);
        Assert.That(stream!.Length, Is.EqualTo(3));
    }

    [Test]
    public void ContentFromBase64String_ShouldReturnNull_WhenBase64IsEmpty()
    {
        var file = new MockFile { Base64String = string.Empty };
        Assert.That(file.ContentFromBase64String(), Is.Null);
    }

    [Test]
    public void Base64StringFromContent_ShouldReturnEquivalentString()
    {
        var bytes = new byte[] { 10, 20, 30 };
        using var stream = new MemoryStream(bytes);
        var file = new MockFile { Content = stream };

        var result = file.Base64StringFromContent();

        Assert.That(result, Is.EqualTo(Convert.ToBase64String(bytes)));
    }

    private class MockFile : IFile
    {
        public string Name { get; set; } = "Test.txt";
        public string Type { get; set; } = "text/plain";
        public Uri? Uri { get; set; }
        public Stream? Content { get; set; }
        public string? Base64String { get; set; }
    }
}