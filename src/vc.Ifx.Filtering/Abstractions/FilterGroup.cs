namespace VisionaryCoder.Framework.Filtering.Abstractions;

public sealed record FilterGroup(FilterCombination Combination, IReadOnlyList<FilterNode> Children) : FilterNode;
