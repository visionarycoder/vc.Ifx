// VisionaryCoder.Framework.Extensions.Querying

using System.Linq.Expressions;
namespace VisionaryCoder.Framework.Querying;

/// <summary>
/// Represents a reusable predicate for querying <typeparamref name="T"/> with LINQ.
/// </summary>
/// <remarks>
/// QueryFilter is a thin wrapper around an expression tree that can be composed using
/// helper extensions. You can build small, focused filters and then combine them.
/// 
/// Example:
/// <example>
/// <code>
/// // Define simple filters
/// var nameHasAnn    = QueryFilterExtensions.Contains&lt;User&gt;(u =&gt; u.Name, "ann");
/// var emailEndsOrg  = QueryFilterExtensions.EndsWithIgnoreCase&lt;User&gt;(u =&gt; u.Email, ".org");
/// 
/// // Combine with AND/OR/NOT
/// var combined = nameHasAnn.And(emailEndsOrg);
/// 
/// // Apply to a queryable
/// IQueryable&lt;User&gt; query = db.Users.AsQueryable();
/// var result = query.Apply(combined).ToList();
/// </code>
/// </example>
/// </remarks>
public sealed class QueryFilter<T>(Expression<Func<T, bool>> predicate)
{
    /// <summary>
    /// The underlying predicate expression.
    /// </summary>
    /// <remarks>
    /// This expression can be directly used in LINQ providers (e.g., EF Core) and
    /// composed via the extension methods in <see cref="QueryFilterExtensions"/>.
    /// </remarks>
    public Expression<Func<T, bool>> Predicate { get; } = predicate ?? throw new ArgumentNullException(nameof(predicate));
}
