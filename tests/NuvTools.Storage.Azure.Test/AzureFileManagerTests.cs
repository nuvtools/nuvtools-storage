using NUnit.Framework;
using System.Threading.Tasks;

namespace NuvTools.Storage.Azure.Test;

[TestFixture()]
public class AzureFileManagerTests
{
    private AzureFileManager _azureFileManager;
    private readonly File _file = new("test", "file/plain", "QlJVTk8gTUVMTw==");
    private readonly File _file2 = new("test", "file/plain", "QlJVTk8gTUVMTw==");

    [SetUp]
    public void Configure()
    {
        _azureFileManager = new AzureFileManager("UseDevelopmentStorage=true", "files");
    }


    [Test()]
    public void GetAccessAccountTokenTest()
    {

    }

    [Test()]
    public void GetAccessRepositoryTokenTest()
    {

    }

    [Test()]
    public async Task GetFileAsyncTestAsync()
    {
        var arquivo = await _azureFileManager.GetFileAsync(_file.Name, true);

        Assert.That(arquivo.Base64String == _file.Base64String);
    }

    [Test()]
    public async Task GetFilesAsyncTestAsync()
    {
        var arquivo = await _azureFileManager.GetFilesAsync(10);

        Assert.That(arquivo is not null);
    }

    [Test()]
    public async Task RemoveFileAsyncTestAsync()
    {
        await _azureFileManager.RemoveFileAsync("test");
    }

    [Test()]
    public async Task AddFileAsyncTestAsync()
    {
        var resultado = await _azureFileManager.AddFileAsync(_file);
        Assert.That(resultado.Uri is not null);
    }

    [Test()]
    public async Task AddFilesAsyncTestAsync()
    {
        var resultado = await _azureFileManager.AddFilesAsync([_file, _file2]);
        Assert.That(resultado.Count > 0);
    }

    [Test()]
    public void FileExistsAsyncTest()
    {

    }
}