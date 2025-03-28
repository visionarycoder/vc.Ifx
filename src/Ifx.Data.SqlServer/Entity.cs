// ReSharper disable UnusedMember.Global

using System.Buffers.Binary;

#pragma warning disable ClassWithData
#pragma warning disable DerivedClasses
namespace vc.Ifx.Data;

public abstract partial class Entity
{
    public ulong RowVersion { get; set; }

}