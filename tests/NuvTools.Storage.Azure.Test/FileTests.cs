using NUnit.Framework;
using System;
using System.IO;

namespace NuvTools.Storage.Azure.Test;

[TestFixture]
public class FileTests
{
    [Test]
    public void Constructor_WithContent_ShouldInitializeBase64()
    {
        using var stream = new MemoryStream([1, 2]);
        var file = new File("test.txt", "text/plain", stream);

        Assert.Multiple(() =>
        {
            Assert.That(file.Content, Is.Not.Null);
            Assert.That(file.Base64String, Is.Not.Null);
        });
    }

    [Test]
    public void Constructor_WithBase64_ShouldInitializeStream()
    {
        var base64 = Convert.ToBase64String(new byte[] { 9, 8, 7 });
        var file = new File("doc.txt", "text/plain", base64);

        Assert.That(file.Content, Is.Not.Null);
        Assert.That(file.Content!.Length, Is.EqualTo(3));
    }

    [Test]
    public void Constructor_WithUri_ShouldInitializeUri()
    {
        var uri = new Uri("https://example.com/test.txt");
        var file = new File("test.txt", "text/plain", uri);

        Assert.That(file.Uri, Is.EqualTo(uri));
    }
}
