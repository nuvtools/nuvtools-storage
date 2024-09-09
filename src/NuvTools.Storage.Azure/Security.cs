using Azure.Storage;
using Azure.Storage.Sas;

namespace NuvTools.Storage.Azure;

public class Security
{
    public static string GetAccessAccountToken(string accountName, string accountKey, AccessPermissions permissions = AccessPermissions.Read)
    {
        AccountSasBuilder sasBuilder = new()
        {
            Services = AccountSasServices.Blobs,
            ResourceTypes = AccountSasResourceTypes.Object,
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(24),
            Protocol = SasProtocol.Https
        };

        sasBuilder.SetPermissions(PermissionsHelper.GetPermissions(permissions));

        return sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(accountName, accountKey)).ToString();
    }
}
