namespace VisionaryCoder.Framework.Storage;

/// <summary>Operations supported by an object storage provider.</summary>
[Flags]
public enum StorageCapabilities
{
    /// <summary>No operations are supported.</summary>
    None = 0,
    /// <summary>Object content can be read.</summary>
    Read = 1,
    /// <summary>Object content can be written or replaced.</summary>
    Write = 2,
    /// <summary>Objects can be deleted.</summary>
    Delete = 4,
    /// <summary>Object metadata can be retrieved.</summary>
    Metadata = 8,
    /// <summary>Objects can be listed by prefix.</summary>
    List = 16,
    /// <summary>Writes can atomically reject replacement of an existing object.</summary>
    CreateOnly = 32
}
