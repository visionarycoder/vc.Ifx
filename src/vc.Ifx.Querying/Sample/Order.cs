// QueryFilterDemo.cs

namespace VisionaryCoder.Framework.Querying.Sample;
// QueryFilter, QueryFilterExtensions

// POCO models
public class Order { public int Id { get; set; } public decimal Total { get; set; } }

// Simple EF Core context (InMemory for demo)

// A consumer receiving a QueryFilter<T>
