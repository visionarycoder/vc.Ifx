// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VisionaryCoder.Framework.Data.EntityFramework;

/// <summary>
/// Extension methods for configuring EntityId types in EF Core model builders.
/// </summary>
public static class EntityIdModelBuilderExtensions
{
    /// <summary>
    /// Configures an EntityId property to use the EntityIdValueConverter for database storage.
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <typeparam name="TKey">The key type</typeparam>
    /// <param name="builder">The property builder</param>
    /// <returns>The property builder for method chaining</returns>
    public static PropertyBuilder<EntityId<TEntity, TKey>> UseEntityId<TEntity, TKey>(this PropertyBuilder<EntityId<TEntity, TKey>> builder)
        where TEntity : class
        where TKey : notnull
    {
        var converter = new EntityIdValueConverter<TEntity, TKey>();
        builder.HasConversion(converter);
        return builder;
    }
}
