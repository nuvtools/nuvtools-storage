
using Azure.Storage.Sas;

namespace NuvTools.Storage.Azure;

/// <summary>
/// Internal helper class for converting NuvTools access permissions to Azure Storage SAS permissions.
/// </summary>
internal class PermissionsHelper
{
    /// <summary>
    /// Converts NuvTools <see cref="AccessPermissions"/> to Azure <see cref="AccountSasPermissions"/>.
    /// </summary>
    /// <param name="permissions">The NuvTools access permissions to convert.</param>
    /// <returns>The corresponding Azure account-level SAS permissions.</returns>
    public static AccountSasPermissions GetAccountSasPermissions(AccessPermissions permissions)
    {
        return permissions switch
        {
            AccessPermissions.Read => AccountSasPermissions.Read,
            AccessPermissions.Add => AccountSasPermissions.Add,
            AccessPermissions.Create => AccountSasPermissions.Create,
            AccessPermissions.Update => AccountSasPermissions.Update,
            AccessPermissions.Write => AccountSasPermissions.Write,
            AccessPermissions.Delete => AccountSasPermissions.Delete,
            AccessPermissions.List => AccountSasPermissions.List,
            _ => AccountSasPermissions.All,
        };
    }

    /// <summary>
    /// Converts NuvTools <see cref="AccessPermissions"/> to Azure <see cref="BlobContainerSasPermissions"/>.
    /// </summary>
    /// <param name="permissions">The NuvTools access permissions to convert.</param>
    /// <returns>The corresponding Azure blob container-level SAS permissions.</returns>
    public static BlobContainerSasPermissions GetContainerSasPermissions(AccessPermissions permissions)
    {
        return permissions switch
        {
            AccessPermissions.Read => BlobContainerSasPermissions.Read,
            AccessPermissions.Add => BlobContainerSasPermissions.Add,
            AccessPermissions.Create => BlobContainerSasPermissions.Create,
            AccessPermissions.Write => BlobContainerSasPermissions.Write,
            AccessPermissions.Delete => BlobContainerSasPermissions.Delete,
            AccessPermissions.List => BlobContainerSasPermissions.List,
            _ => BlobContainerSasPermissions.All,
        };
    }

    /// <summary>
    /// Converts NuvTools <see cref="AccessPermissions"/> to Azure <see cref="BlobSasPermissions"/>.
    /// </summary>
    /// <param name="permissions">The NuvTools access permissions to convert.</param>
    /// <returns>The corresponding Azure blob-level SAS permissions.</returns>
    public static BlobSasPermissions GetBlobSasPermissions(AccessPermissions permissions)
    {
        return permissions switch
        {
            AccessPermissions.Read => BlobSasPermissions.Read,
            AccessPermissions.Add => BlobSasPermissions.Add,
            AccessPermissions.Create => BlobSasPermissions.Create,
            AccessPermissions.Write => BlobSasPermissions.Write,
            AccessPermissions.Delete => BlobSasPermissions.Delete,
            AccessPermissions.List => BlobSasPermissions.List,
            _ => BlobSasPermissions.All,
        };
    }
}
