using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VisionaryCoder.Framework.Primitives.Data.EFCore;

public static class EntityIdModelBuilderExtensions
{
    public static PropertyBuilder<EntityId<TEntity, TKey>> UseEntityId<TEntity, TKey>(this PropertyBuilder<EntityId<TEntity, TKey>> builder)
        where TEntity : class
        where TKey : notnull
    {
        var converter = new EntityIdValueConverter<TEntity, TKey>();
        builder.HasConversion(converter);
        return builder;
    }
}
