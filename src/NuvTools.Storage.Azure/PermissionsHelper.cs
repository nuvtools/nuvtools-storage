
using Azure.Storage.Sas;

namespace NuvTools.Storage.Azure;

internal class PermissionsHelper
{
    public static AccountSasPermissions GetPermissions(AccessPermissions permissions)
    {
        switch (permissions)
        {
            case AccessPermissions.Read:
                return AccountSasPermissions.Read;
            case AccessPermissions.Add:
                return AccountSasPermissions.Add;
            case AccessPermissions.Create:
                return AccountSasPermissions.Create;
            case AccessPermissions.Update:
                return AccountSasPermissions.Update;
            case AccessPermissions.Write:
                return AccountSasPermissions.Write;
            case AccessPermissions.Delete:
                return AccountSasPermissions.Delete;
            case AccessPermissions.List:
                return AccountSasPermissions.List;
            default:
                return AccountSasPermissions.All;
        }
    }

    public static BlobContainerSasPermissions GetPermissionsBlob(AccessPermissions permissions)
    {
        switch (permissions)
        {
            case AccessPermissions.Read:
                return BlobContainerSasPermissions.Read;
            case AccessPermissions.Add:
                return BlobContainerSasPermissions.Add;
            case AccessPermissions.Create:
                return BlobContainerSasPermissions.Create;
            case AccessPermissions.Write:
                return BlobContainerSasPermissions.Write;
            case AccessPermissions.Delete:
                return BlobContainerSasPermissions.Delete;
            case AccessPermissions.List:
                return BlobContainerSasPermissions.List;
            default:
                return BlobContainerSasPermissions.All;
        }
    }
}
