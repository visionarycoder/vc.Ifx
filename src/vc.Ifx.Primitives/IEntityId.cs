namespace VisionaryCoder.Framework.Primitives;

public interface IEntityId
{
    Type ValueType { get; }
    object BoxedValue { get; }
}
