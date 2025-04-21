// ReSharper disable UnusedMember.Global

#pragma warning disable ClassWithData
#pragma warning disable DerivedClasses
namespace vc.Ifx.Data;

public abstract partial class Entity
{
    public ulong RowVersion { get; set; }

}