using System.Collections.Frozen;
using VisionaryCoder.Framework.Filtering.Abstractions;

namespace VisionaryCoder.Framework.Querying.Serialization;

internal static class QueryFilterOperations
{
    internal static readonly FrozenDictionary<string, FilterOperation> All = new Dictionary<string, FilterOperation>(StringComparer.Ordinal)
    {
        ["Equals"] = FilterOperation.Equals,
        ["NotEquals"] = FilterOperation.NotEquals,
        ["GreaterThan"] = FilterOperation.GreaterThan,
        ["GreaterThanOrEqual"] = FilterOperation.GreaterOrEqual,
        ["LessThan"] = FilterOperation.LessThan,
        ["LessThanOrEqual"] = FilterOperation.LessOrEqual,
        ["Contains"] = FilterOperation.Contains,
        ["StartsWith"] = FilterOperation.StartsWith,
        ["EndsWith"] = FilterOperation.EndsWith,
        ["In"] = FilterOperation.In,
        ["NotIn"] = FilterOperation.In
    }.ToFrozenDictionary(StringComparer.Ordinal);
}
