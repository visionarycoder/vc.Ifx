using Ifx.Primitives;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ifx.Data.EntityFrameworkCore;

public sealed class EntityIdentifierValueConverter<TEntity, TKey>()
    : ValueConverter<EntityIdentifier<TEntity, TKey>, TKey>(id => id.Value, v => new EntityIdentifier<TEntity, TKey>(v))
    where TEntity : class
    where TKey : notnull;
