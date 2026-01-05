// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace VisionaryCoder.Framework.Data.EntityFramework;

/// <summary>
/// EF Core value converter for EntityId types.
/// Converts between EntityId&lt;TEntity, TKey&gt; and its underlying TKey value for database storage.
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The key type</typeparam>
public sealed class EntityIdValueConverter<TEntity, TKey>() : ValueConverter<EntityId<TEntity, TKey>, TKey>(id => id.Value, v => new EntityId<TEntity, TKey>(v)) 
    where TEntity : class 
    where TKey : notnull
{
}
