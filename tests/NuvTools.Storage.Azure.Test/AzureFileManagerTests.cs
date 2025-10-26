using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NuvTools.Storage.Azure.Test
{
    [TestFixture]
    public class AzureFileManagerTests
    {
        private Mock<BlobServiceClient>? _mockServiceClient;
        private Mock<BlobContainerClient>? _mockContainerClient;
        private Mock<BlobClient>? _mockBlobClient;
        private AzureFileManager? _fileManager;
        private File? _sampleFile;

        [SetUp]
        public void SetUp()
        {
            _mockServiceClient = new Mock<BlobServiceClient>();
            _mockContainerClient = new Mock<BlobContainerClient>();
            _mockBlobClient = new Mock<BlobClient>();

            _mockServiceClient
                .Setup(s => s.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_mockContainerClient.Object);

            _mockContainerClient
                .Setup(c => c.GetBlobClient(It.IsAny<string>()))
                .Returns(_mockBlobClient.Object);

            _fileManager = new AzureFileManager(_mockServiceClient.Object, "files");

            _sampleFile = new File("sample.txt", "text/plain", "QlJVTk8gTUVMTw==");

            _mockBlobClient
                .SetupGet(b => b.Name)
                .Returns("sample.txt");

            _mockBlobClient
                .SetupGet(b => b.Uri)
                .Returns(new Uri("https://fake.blob.core.windows.net/files/sample.txt"));

            _mockBlobClient
                .Setup(b => b.GetPropertiesAsync(
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(
                    BlobsModelFactory.BlobProperties(contentType: "text/plain"),
                    Mock.Of<Response>()));

            _mockBlobClient
                .Setup(b => b.DownloadToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Mock.Of<Response>()));

            _mockBlobClient
                .Setup(b => b.DownloadToAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<BlobDownloadToOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Mock.Of<Response>()));

            _mockBlobClient
                .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Response.FromValue((BlobContentInfo?)null, Mock.Of<Response>())));

            _mockBlobClient
                .Setup(b => b.DeleteIfExistsAsync(
                    It.IsAny<DeleteSnapshotsOption>(),
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

            _mockBlobClient
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));
        }

        [Test]
        public void GetAccessRepositoryUri_ShouldReturnValidUri()
        {
            var fakeUri = new Uri("https://fake.blob.core.windows.net/files?sig=fake");
            _mockContainerClient!
                .Setup(c => c.GenerateSasUri(It.IsAny<BlobContainerSasPermissions>(), It.IsAny<DateTimeOffset>()))
                .Returns(fakeUri);

            var uri = _fileManager!.GetAccessRepositoryUri();

            Assert.That(uri, Is.EqualTo(fakeUri));
        }

        [Test]
        public async Task AddFileAsync_ShouldUploadAndReturnFile()
        {
            // Arrange
            _mockBlobClient!
                .SetupGet(b => b.Name)
                .Returns("sample.txt");

            _mockBlobClient
                .SetupGet(b => b.Uri)
                .Returns(new Uri("https://fake.blob.core.windows.net/files/sample.txt"));

            _mockBlobClient
                .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Response.FromValue((BlobContentInfo?)null, Mock.Of<Response>())));

            _mockBlobClient
                .Setup(b => b.GetPropertiesAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(
                    BlobsModelFactory.BlobProperties(contentType: "text/plain"),
                    Mock.Of<Response>()));

            // Act
            var result = await _fileManager!.AddFileAsync(_sampleFile!);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo(_sampleFile!.Name));
            Assert.That(result.Uri, Is.Not.Null);
        }


        [Test]
        public async Task AddFilesAsync_ShouldUploadMultipleFiles()
        {
            var file1 = new File("file1.txt", "text/plain", "QlJVTk8gTUVMTw==");
            var file2 = new File("file2.txt", "text/plain", "QlJVTk8gTUVMTw==");

            _mockBlobClient!
                .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Response.FromValue((BlobContentInfo?)null, Mock.Of<Response>())));

            var result = await _fileManager!.AddFilesAsync(new[] { file1, file2 });

            Assert.That(result, Has.Length.EqualTo(2));
        }

        [Test]
        public async Task GetFileAsync_ShouldReturnNull_WhenBlobDoesNotExist()
        {
            _mockBlobClient!
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));

            var result = await _fileManager!.GetFileAsync("unknown.txt");
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetFilesAsync_ShouldThrow_WhenContainerNotExists()
        {
            _mockContainerClient!
                .Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));

            await Task.Yield();

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _fileManager!.GetFilesAsync(10));
        }

        [Test]
        public async Task RemoveFileAsync_ShouldCallDeleteIfExists()
        {
            _mockBlobClient!
                .Setup(b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

            await _fileManager!.RemoveFileAsync("sample.txt");

            _mockBlobClient.Verify(
                b => b.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), null, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task FileExistsAsync_ShouldReturnTrue_WhenExists()
        {
            _mockBlobClient!
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

            bool exists = await _fileManager!.FileExistsAsync("sample.txt");

            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task FileExistsAsync_ShouldReturnFalse_WhenNotExists()
        {
            _mockBlobClient!
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));

            bool exists = await _fileManager!.FileExistsAsync("unknown.txt");

            Assert.That(exists, Is.False);
        }
    }
}
