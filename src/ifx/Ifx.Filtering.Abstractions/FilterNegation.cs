namespace Ifx.Filtering.Abstractions;

/// <summary>Logical negation of a complete predicate.</summary>
public sealed record FilterNegation(FilterNode Operand) : FilterNode;
