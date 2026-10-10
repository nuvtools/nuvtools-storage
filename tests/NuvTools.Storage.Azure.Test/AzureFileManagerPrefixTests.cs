using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;
using NUnit.Framework;

namespace NuvTools.Storage.Azure.Test
{
    /// <summary>
    /// Covers the operations by path and by prefix: listing, downloading to a stream, copying, moving
    /// and removing.
    /// </summary>
    [TestFixture]
    public class AzureFileManagerPrefixTests
    {
        private const string ContainerUri = "https://fake.blob.core.windows.net/files";

        private Mock<BlobContainerClient>? _container;
        private AzureFileManager? _fileManager;
        private Dictionary<string, Mock<BlobClient>>? _blobs;

        [SetUp]
        public void SetUp()
        {
            _blobs = [];
            _container = new Mock<BlobContainerClient>();
            _container
                .Setup(c => c.GetBlobClient(It.IsAny<string>()))
                .Returns((string name) => Blob(name).Object);

            var service = new Mock<BlobServiceClient>();
            service.Setup(s => s.GetBlobContainerClient(It.IsAny<string>())).Returns(_container.Object);

            _fileManager = new AzureFileManager(service.Object, "files");
        }

        /// <summary>The mock of one blob, created on first use: existing, readable and writable.</summary>
        private Mock<BlobClient> Blob(string name)
        {
            if (_blobs!.TryGetValue(name, out var existing))
                return existing;

            var blob = new Mock<BlobClient>();
            blob.SetupGet(b => b.Name).Returns(name);
            blob.SetupGet(b => b.Uri).Returns(new Uri($"{ContainerUri}/{name}"));
            blob.Setup(b => b.GetPropertiesAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(BlobsModelFactory.BlobProperties(contentType: "application/pdf"), Mock.Of<Response>()));
            blob.Setup(b => b.OpenReadAsync(It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new MemoryStream([1, 2, 3]));
            blob.Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue((BlobContentInfo)null!, Mock.Of<Response>()));
            blob.Setup(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

            _blobs[name] = blob;
            return blob;
        }

        /// <summary>Makes a blob answer every read as the service does for one that is not there.</summary>
        private void Missing(string name)
        {
            var notFound = new RequestFailedException(404, "The specified blob does not exist.");
            var blob = Blob(name);
            blob.Setup(b => b.GetPropertiesAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>())).ThrowsAsync(notFound);
            blob.Setup(b => b.DownloadToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).ThrowsAsync(notFound);
        }

        /// <summary>Sets what the repository lists under a prefix.</summary>
        private void Listing(string? prefix, params string[] names)
        {
            var items = names
                .Select(name => BlobsModelFactory.BlobItem(
                    name: name,
                    properties: BlobsModelFactory.BlobItemProperties(
                        accessTierInferred: false,
                        contentLength: 42,
                        contentType: "application/pdf",
                        lastModified: new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero))))
                .ToList();

            var pages = AsyncPageable<BlobItem>.FromPages([Page<BlobItem>.FromValues(items, null, Mock.Of<Response>())]);

            _container!
                .Setup(c => c.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), prefix, It.IsAny<CancellationToken>()))
                .Returns(pages);
        }

        private void VerifyUploaded(string name, Times times)
            => Blob(name).Verify(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()), times);

        private void VerifyDeleted(string name, Times times)
            => Blob(name).Verify(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()), times);

        [Test]
        public async Task ListFilesAsync_ShouldDescribeTheFilesUnderThePrefix()
        {
            Listing("docs/", "docs/a.pdf", "docs/b.pdf");

            var files = new List<FileDescriptor>();
            await foreach (var file in _fileManager!.ListFilesAsync("docs/"))
                files.Add(file);

            Assert.Multiple(() =>
            {
                Assert.That(files.Select(f => f.Path), Is.EqualTo(new[] { "docs/a.pdf", "docs/b.pdf" }));
                Assert.That(files[0].Size, Is.EqualTo(42));
                Assert.That(files[0].ContentType, Is.EqualTo("application/pdf"));
                Assert.That(files[0].LastModified, Is.EqualTo(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero)));
            });
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task ListFilesAsync_WithoutAPrefix_ShouldListEverything(string? prefix)
        {
            Listing(null, "a.pdf", "docs/b.pdf");

            var paths = new List<string>();
            await foreach (var file in _fileManager!.ListFilesAsync(prefix))
                paths.Add(file.Path);

            Assert.That(paths, Is.EqualTo(new[] { "a.pdf", "docs/b.pdf" }));
        }

        [Test]
        public async Task DownloadFileAsync_ShouldWriteToTheStream()
        {
            Blob("docs/a.pdf")
                .Setup(b => b.DownloadToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback((Stream destination, CancellationToken _) => destination.Write([7, 8, 9]))
                .ReturnsAsync(Mock.Of<Response>());

            using var destination = new MemoryStream();
            var found = await _fileManager!.DownloadFileAsync("docs/a.pdf", destination);

            Assert.Multiple(() =>
            {
                Assert.That(found, Is.True);
                Assert.That(destination.ToArray(), Is.EqualTo(new byte[] { 7, 8, 9 }));
            });
        }

        [Test]
        public async Task DownloadFileAsync_ShouldReturnFalse_WhenTheFileDoesNotExist()
        {
            Missing("docs/gone.pdf");

            using var destination = new MemoryStream();
            var found = await _fileManager!.DownloadFileAsync("docs/gone.pdf", destination);

            Assert.That(found, Is.False);
        }

        [Test]
        public async Task CopyFileAsync_ShouldWriteTheTargetAndKeepTheSource()
        {
            var copy = await _fileManager!.CopyFileAsync("docs/a.pdf", "archive/a.pdf");

            Assert.Multiple(() =>
            {
                Assert.That(copy.Name, Is.EqualTo("archive/a.pdf"));
                Assert.That(copy.Type, Is.EqualTo("application/pdf"));
                Assert.That(copy.Uri, Is.EqualTo(new Uri($"{ContainerUri}/archive/a.pdf")));
            });
            VerifyUploaded("archive/a.pdf", Times.Once());
            VerifyDeleted("docs/a.pdf", Times.Never());
        }

        [Test]
        public void CopyFileAsync_ShouldThrowFileNotFound_WhenTheSourceDoesNotExist()
        {
            Missing("docs/gone.pdf");

            Assert.ThrowsAsync<FileNotFoundException>(() => _fileManager!.CopyFileAsync("docs/gone.pdf", "archive/gone.pdf"));
            VerifyUploaded("archive/gone.pdf", Times.Never());
        }

        [Test]
        public void CopyFileAsync_ShouldRefuseTheSamePath()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _fileManager!.CopyFileAsync("docs/a.pdf", "docs/a.pdf"));
        }

        [Test]
        public async Task MoveFileAsync_ShouldWriteTheTargetAndRemoveTheSource()
        {
            var moved = await _fileManager!.MoveFileAsync("docs/a.pdf", "docs/renamed.pdf");

            Assert.That(moved.Name, Is.EqualTo("docs/renamed.pdf"));
            VerifyUploaded("docs/renamed.pdf", Times.Once());
            VerifyDeleted("docs/a.pdf", Times.Once());
        }

        [Test]
        public void MoveFileAsync_ShouldLeaveTheSource_WhenTheCopyFails()
        {
            Blob("docs/renamed.pdf")
                .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(500, "The upload failed."));

            Assert.ThrowsAsync<RequestFailedException>(() => _fileManager!.MoveFileAsync("docs/a.pdf", "docs/renamed.pdf"));
            VerifyDeleted("docs/a.pdf", Times.Never());
        }

        [Test]
        public async Task MoveFilesAsync_ShouldMoveEveryFileKeepingTheRestOfThePath()
        {
            Listing("drafts/", "drafts/a.pdf", "drafts/sub/b.pdf");

            var moved = await _fileManager!.MoveFilesAsync("drafts/", "final/");

            Assert.That(moved, Is.EqualTo(2));
            VerifyUploaded("final/a.pdf", Times.Once());
            VerifyUploaded("final/sub/b.pdf", Times.Once());
            VerifyDeleted("drafts/a.pdf", Times.Once());
            VerifyDeleted("drafts/sub/b.pdf", Times.Once());
        }

        [Test]
        public async Task MoveFilesAsync_ShouldDoNothing_WhenThePrefixIsEmptyOfFiles()
        {
            Listing("drafts/");

            var moved = await _fileManager!.MoveFilesAsync("drafts/", "final/");

            Assert.That(moved, Is.Zero);
        }

        [Test]
        public void MoveFilesAsync_ShouldRefuseTheSamePrefix()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _fileManager!.MoveFilesAsync("drafts/", "drafts/"));
        }

        [Test]
        public async Task RemoveFilesAsync_ShouldRemoveEveryFileUnderThePrefix()
        {
            Listing("drafts/", "drafts/a.pdf", "drafts/b.pdf");

            var removed = await _fileManager!.RemoveFilesAsync("drafts/");

            Assert.That(removed, Is.EqualTo(2));
            VerifyDeleted("drafts/a.pdf", Times.Once());
            VerifyDeleted("drafts/b.pdf", Times.Once());
        }

        [TestCase("")]
        [TestCase("  ")]
        public void RemoveFilesAsync_ShouldRefuseAnEmptyPrefix(string prefix)
        {
            Assert.ThrowsAsync<ArgumentException>(() => _fileManager!.RemoveFilesAsync(prefix));
            _container!.Verify(c => c.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}
