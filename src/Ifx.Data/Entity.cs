// ReSharper disable UnusedMember.Global
#pragma warning disable ClassWithData
#pragma warning disable DerivedClasses
namespace vc.Ifx.Data;

public abstract partial class Entity
{

    public const string UNDEFINED = "Undefined";

    public int Id { get; set; }

    public string Created { get; set; } = UNDEFINED;
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    
    public string Updated { get; set; } = UNDEFINED;
    public DateTime UpdatedOnUtc { get; set; } = DateTime.UtcNow;

}