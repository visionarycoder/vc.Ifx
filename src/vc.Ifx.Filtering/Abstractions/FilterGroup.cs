namespace VisionaryCoder.Framework.Filtering.Abstractions;

public sealed record FilterGroup(FilterCombination Combination, IReadOnlyList<FilterNode> Children) : FilterNode
{
    /// <summary>Gets an immutable snapshot of child predicates.</summary>
    public IReadOnlyList<FilterNode> Children
    {
        get;
        init => field = Array.AsReadOnly((value ?? throw new ArgumentNullException(nameof(Children))).ToArray());
    } = Array.AsReadOnly((Children ?? throw new ArgumentNullException(nameof(Children))).ToArray());
}
