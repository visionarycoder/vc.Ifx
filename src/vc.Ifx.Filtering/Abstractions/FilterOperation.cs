namespace VisionaryCoder.Framework.Filtering.Abstractions;

/// <summary>
/// Represents an operation used in filter conditions.
/// </summary>
/// <remarks>
/// This enum is a consolidated operation type used by <see cref="FilterCondition"/> and
/// <see cref="FilterCollectionCondition"/>. For clarity, more specific operator enums are
/// also provided (<see cref="ComparisonOperator"/>, <see cref="StringOperator"/>, <see cref="CollectionOperator"/>).
/// </remarks>
public enum FilterOperation
{
    // Comparison operators
    Equals,
    NotEquals,
    GreaterThan,
    GreaterOrEqual,
    LessThan,
    LessOrEqual,

    // String operators
    Contains,
    StartsWith,
    EndsWith,

    // Collection operators
    Any,
    All,
    HasElements,

    // Membership
    In
}
