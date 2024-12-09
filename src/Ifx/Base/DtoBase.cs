#pragma warning disable DerivedClasses
#pragma warning disable ClassWithData

namespace vc.Ifx.Base;

/// <summary>
/// Represents the base class for Data Transfer Objects (DTOs).
/// </summary>
public abstract class DtoBase
{

    public Guid InstanceId { get; set; } = Guid.NewGuid();

}
