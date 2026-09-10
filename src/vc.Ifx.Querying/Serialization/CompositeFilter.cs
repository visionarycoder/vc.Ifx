namespace VisionaryCoder.Framework.Querying.Serialization;
// "And", "Or", "Not"
public sealed record CompositeFilter(string Operator, List<FilterNode> Children) : FilterNode;
