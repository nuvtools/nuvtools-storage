
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
    public static AccountSasPermissions GetPermissions(AccessPermissions permissions)
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
    public static BlobContainerSasPermissions GetPermissionsBlob(AccessPermissions permissions)
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
}
