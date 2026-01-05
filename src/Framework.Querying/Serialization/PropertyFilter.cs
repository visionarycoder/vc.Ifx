namespace VisionaryCoder.Framework.Querying.Serialization;

public sealed record PropertyFilter(string Operator, string Property, string? Value, bool IgnoreCase = false) : FilterNode;
