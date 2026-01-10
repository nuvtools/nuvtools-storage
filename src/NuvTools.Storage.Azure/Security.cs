using Azure.Storage;
using Azure.Storage.Sas;

namespace NuvTools.Storage.Azure;

/// <summary>
/// Provides security-related functionality for Azure Storage, including SAS token generation.
/// </summary>
public class Security
{
    /// <summary>
    /// Generates a signed token for accessing Azure Storage account resources.
    /// The token is valid for 24 hours and uses HTTPS protocol.
    /// </summary>
    /// <param name="accountName">The name of the Azure Storage account.</param>
    /// <param name="accountKey">The access key for the Azure Storage account.</param>
    /// <param name="permissions">The access permissions to grant. Defaults to <see cref="AccessPermissions.Read"/>.</param>
    /// <returns>A signed token string that can be appended to storage URLs for authenticated access.</returns>
    public static string GetAccountSignedToken(string accountName, string accountKey, AccessPermissions permissions = AccessPermissions.Read)
    {
        AccountSasBuilder sasBuilder = new()
        {
            Services = AccountSasServices.Blobs,
            ResourceTypes = AccountSasResourceTypes.Object,
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(24),
            Protocol = SasProtocol.Https
        };

        sasBuilder.SetPermissions(PermissionsHelper.GetAccountSasPermissions(permissions));

        return sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(accountName, accountKey)).ToString();
    }
}
