using System.Runtime.Serialization;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

#pragma warning disable DerivedClasses
#pragma warning disable ClassWithData

namespace Ifx.Services;

/// <summary>
///     Represents the base class for classes being passed across service boundaries.
/// </summary>
[DataContract]
public abstract class DataContractBase : IExtensibleDataObject
{
    [DataMember] public int Id { get; set; }
    [DataMember] public Guid Uuid { get; set; } = Guid.NewGuid();

    // The extensionDataValue field holds data from future versions
    // of the type.  This enables this type to be compatible with
    // future versions. The field is required to implement the
    // IExtensibleDataObject interface.
    // https://learn.microsoft.com/en-us/dotnet/api/system.runtime.serialization.datamemberattribute?view=net-9.0
    [IgnoreDataMember] public ExtensionDataObject ExtensionData { get; set; }
}
