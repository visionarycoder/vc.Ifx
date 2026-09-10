---
title: Collection Operations in ExpressionToFilterNode
doc_type: reference
status: active
last_updated: 2026-09-10
---
# Collection Operations in ExpressionToFilterNode

## Overview

The `ExpressionToFilterNode` class now supports LINQ collection methods (`Any()`, `All()`, and `Contains()`) when translating C# expressions to FilterNode structures. This enables powerful filtering capabilities on collection properties.

## Supported Collection Operations

### Any() - Without Predicate

Check if a collection has any elements.

```csharp
// Expression
Expression<Func<Customer, bool>> expr = c => c.Orders.Any();

// Translates to
FilterCollectionCondition(
    Path: "Orders",
    Operator: FilterOperation.HasElements,
    Predicate: null
)
```

### Any() - With Predicate

Check if any element in the collection matches a condition.

```csharp
// Expression
Expression<Func<Customer, bool>> expr = c => c.Orders.Any(o => o.Total > 1000);

// Translates to
FilterCollectionCondition(
    Path: "Orders",
    Operator: FilterOperation.Any,
    Predicate: FilterCondition(
        Path: "Total",
        Operator: FilterOperation.GreaterThan,
        Value: "1000"
    )
)
```

### All() - With Predicate

Check if all elements in the collection match a condition.

```csharp
// Expression
Expression<Func<Customer, bool>> expr = c => c.Orders.All(o => o.IsPaid);

// Translates to
FilterCollectionCondition(
    Path: "Orders",
    Operator: FilterOperation.All,
    Predicate: FilterCondition(
        Path: "IsPaid",
        Operator: FilterOperation.Equals,
        Value: "True"
    )
)
```

### Contains() - Collection Contains Value

Check if a collection contains a specific value.

```csharp
// Expression
Expression<Func<Product, bool>> expr = p => p.Tags.Contains("electronics");

// Translates to
FilterCondition(
    Path: "Tags",
    Operator: FilterOperation.Contains,
    Value: "electronics"
)
```

## Complex Predicates

Collection operations support complex nested predicates with logical operators.

```csharp
// Expression with multiple conditions
Expression<Func<Customer, bool>> expr = c => 
    c.Orders.Any(o => o.Total > 1000 && o.Status == "Pending");

// Translates to
FilterCollectionCondition(
    Path: "Orders",
    Operator: FilterOperation.Any,
    Predicate: FilterGroup(
        Combination: FilterCombination.And,
        Children: [
            FilterCondition("Total", GreaterThan, "1000"),
            FilterCondition("Status", Equals, "Pending")
        ]
    )
)
```

## Combining Collection and Regular Filters

Collection operations can be combined with regular property filters.

```csharp
// Expression combining multiple filters
Expression<Func<Customer, bool>> expr = c => 
    c.Name.Contains("Smith") && 
    c.Orders.Any(o => o.Total > 1000);

// Translates to
FilterGroup(
    Combination: FilterCombination.And,
    Children: [
        FilterCondition("Name", Contains, "Smith"),
        FilterCollectionCondition("Orders", Any, 
            FilterCondition("Total", GreaterThan, "1000"))
    ]
)
```

## New Filter Types

### FilterCollectionCondition

A new `FilterNode` type that represents operations on collection properties.

```csharp
public sealed record FilterCollectionCondition(
    string Path,              // Collection property path
    FilterOperation Operator,  // Any, All, or HasElements
    FilterNode? Predicate     // Nested filter for collection elements
) : FilterNode;
```

### New FilterOperation Values

Three new operators have been added to support collection operations:

- `FilterOperation.Any` - At least one element matches the predicate
- `FilterOperation.All` - All elements match the predicate
- `FilterOperation.HasElements` - Collection is not empty

## Extensibility for Custom Methods

The implementation includes a placeholder for custom method support. To add support for custom methods:

```csharp
static FilterNode? TranslateMethodCall(MethodCallExpression call)
{
    // ... existing code ...
    
    // Custom method example
    if (call.Method.DeclaringType == typeof(MyCustomExtensions) 
        && call.Method.Name == "IsSpecial")
    {
        var targetMember = GetMember(call.Object);
        var path = GetMemberPath(targetMember);
        // Create appropriate FilterNode based on method semantics
        return new FilterCondition(path, FilterOperation.Equals, "special");
    }
    
    return null;
}
```

## Implementation Notes

1. **Instance vs. Extension Methods**: Both `list.Contains(value)` and `Enumerable.Contains(list, value)` are supported.

2. **Type Safety**: The implementation checks if a type is a collection by verifying it implements `IEnumerable<T>`, `ICollection<T>`, or `IList<T>`, while explicitly excluding `string`.

3. **Predicate Translation**: Predicates in `Any()` and `All()` are recursively translated, supporting arbitrarily complex nested filters.

4. **Performance**: The translation is done at expression tree level, allowing query providers (like EF Core) to optimize the resulting queries.

## Usage Example

```csharp
// Define your entity
public class Customer
{
    public string Name { get; set; }
    public List<Order> Orders { get; set; }
}

public class Order
{
    public decimal Total { get; set; }
    public string Status { get; set; }
}

// Build a filter using the FilterBuilder
var filter = Filter.For<Customer>()
    .Where(c => c.Name.Contains("Smith"))
    .Where(c => c.Orders.Any(o => o.Total > 1000))
    .Build();

// The filter can now be serialized, stored, or applied to queries
```

## Testing

Comprehensive unit tests have been added in `ExpressionToFilterNodeTests.cs` covering:
- Basic comparison operations
- String methods (Contains, StartsWith, EndsWith)
- Logical operators (And, Or, Not)
- Collection methods (Any, All, Contains)
- Complex nested conditions
- Edge cases and error handling

## Future Enhancements

Potential extensions to consider:
- `Count()` operator for collection size comparisons
- `First()` / `FirstOrDefault()` for accessing specific elements
- `Select()` for projection within filters
- Support for nested collection navigation (e.g., `Orders.SelectMany(o => o.Items)`)
