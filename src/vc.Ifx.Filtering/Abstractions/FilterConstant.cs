namespace VisionaryCoder.Framework.Filtering.Abstractions;

/// <summary>A predicate that always evaluates to its Boolean value.</summary>
public sealed record FilterConstant(bool Value) : FilterNode;
