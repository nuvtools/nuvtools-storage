namespace NuvTools.Storage;

[Flags]
public enum AccessPermissions
{
    /// <summary>
    /// Complete access.
    /// </summary>
    All = 0,


    /// <summary>
    /// Permission to read resources and list queues and tables granted.
    /// </summary>
    Read = 1,


    /// <summary>
    /// Permission to add messages, table entities, blobs, and files granted.
    /// </summary>
    Add = 2,


    /// <summary>
    /// Permission to create containers, blobs, shares, directories, and files granted.
    /// </summary>
    Create = 3,


    /// <summary>
    /// Permissions to update messages and table entities granted.
    /// </summary>
    Update = 4,

    /// <summary>
    /// Permission to write resources granted.
    /// </summary>
    Write = 5,

    /// <summary>
    /// Permission to delete resources granted.
    /// </summary>
    Delete = 6,

    /// <summary>
    /// Permission to list blob containers, blobs, shares, directories, and files granted.
    /// </summary>
    List = 7
}
