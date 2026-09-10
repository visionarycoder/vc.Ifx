using System.Text.Json.Serialization;

namespace VisionaryCoder.Framework.Filtering.Abstractions;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(FilterCondition), "condition")]
[JsonDerivedType(typeof(FilterGroup), "group")]
[JsonDerivedType(typeof(FilterCollectionCondition), "collection")]
[JsonDerivedType(typeof(FilterConstant), "constant")]
[JsonDerivedType(typeof(FilterNegation), "not")]
public abstract record FilterNode;
