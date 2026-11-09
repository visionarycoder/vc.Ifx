using Ifx.Primitives;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ifx.Data.EntityFrameworkCore;

public static class EntityIdentifierBuilderExtensions
{
    public static PropertyBuilder<EntityIdentifier<TEntity, TKey>> UseEntityId<TEntity, TKey>(this PropertyBuilder<EntityIdentifier<TEntity, TKey>> builder)
        where TEntity : class
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(builder);
        var converter = new EntityIdentifierValueConverter<TEntity, TKey>();
        builder.HasConversion(converter);
        return builder;
    }
}
