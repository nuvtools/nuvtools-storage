using NUnit.Framework;
using System.Threading.Tasks;

namespace NuvTools.Storage.Azure.Test;

[TestFixture()]
public class AzureFileManagerTests
{
    private AzureFileManager _azureFileManager;
    private readonly File _arquivo = new("teste", "file/plain", "QlJVTk8gTUVMTw==");

    [SetUp]
    public void Configure()
    {
        _azureFileManager = new AzureFileManager("UseDevelopmentStorage=true");
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
        var arquivo = await _azureFileManager.GetFileAsync(_arquivo.Name, true);

        Assert.AreEqual(arquivo.Base64String, _arquivo.Base64String);
    }

    [Test()]
    public async Task GetFilesAsyncTestAsync()
    {
        var arquivo = await _azureFileManager.GetFilesAsync(10);

        Assert.IsNotNull(arquivo);
    }

    [Test()]
    public async Task RemoveFileAsyncTestAsync()
    {
        await _azureFileManager.RemoveFileAsync("teste");
    }

    [Test()]
    public async Task AddFileAsyncTestAsync()
    {
        
        var resultado = await _azureFileManager.AddFileAsync(_arquivo);

        Assert.Greater(resultado.Count, 0);
    }

    [Test()]
    public void FileExistsAsyncTest()
    {

    }
}