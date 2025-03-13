
using Azure.Storage.Sas;

namespace NuvTools.Storage.Azure;

internal class PermissionsHelper
{
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
