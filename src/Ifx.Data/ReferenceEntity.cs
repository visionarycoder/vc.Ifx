#pragma warning disable ClassWithData
#pragma warning disable DerivedClasses
namespace vc.Ifx.Data;

public abstract class ReferenceEntity : Entity
{
    public string Value { get; set; } = string.Empty;
}