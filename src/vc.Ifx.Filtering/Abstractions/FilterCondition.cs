namespace VisionaryCoder.Framework.Filtering.Abstractions;

/// <summary>
/// Represents a simple property filter condition (e.g. "Age > 18" or "Name Contains 'x'").
/// </summary>
/// <param name="Path">Dotted property path (e.g. "Address.City").</param>
/// <param name="Operator">The operation to apply for the condition. Uses <see cref="FilterOperation"/>.</param>
/// <param name="Value">The value to compare against, represented as a string (may be null for certain operators).</param>
public sealed record FilterCondition(string Path, FilterOperation Operator, string? Value) : FilterNode;
