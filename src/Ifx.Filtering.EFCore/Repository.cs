using System.Diagnostics;
using Ifx.Filtering.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// ReSharper disable MethodSupportsCancellation

namespace Ifx.Filtering.EFCore;

public class Repository<TDbContext>(ILogger logger, TDbContext ctx) where TDbContext 
    : DbContext
{
    public async Task<ICollection<T>> FindAsync<T>(Filter<T> filter, CancellationToken cancellationToken = default) where T : class, new()
    {
        try
        {
            var query = ctx.Set<T>().AsQueryable();
            query = query.ApplyFilter(filter);
            var list = await query.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
            return list;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while finding entities. {typeof(T).FullName}");
            Debug.WriteLine(ex);
            return new List<T>();
        }
    }

    public async Task<T?> AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            ctx.ChangeTracker.AutoDetectChangesEnabled = true;
            var entityEntry = ctx.Add(entity);
            var count = await ctx.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger.LogDebug(count == 0
                ? $"Add was not applied for {entity}"
                : $"Add was applied for {entity}");
            return entityEntry.Entity;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding the entity.");
            Debug.WriteLine(ex);
            return null;
        }
    }

    public async Task<T?> UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            ctx.ChangeTracker.AutoDetectChangesEnabled = true;
            var idProperty = ctx.Entry(entity).Property("Id");
            if (idProperty.CurrentValue is not int id)
            {
                logger.LogError($"Entity {typeof(T).Name} does not have a valid numeric Id property.");
                return null;
            }

            var existingEntity = await ctx.Set<T>().FindAsync(id).ConfigureAwait(false);
            logger.LogDebug(existingEntity == null
                ? $"Unable to find {typeof(T).Name}(id={id}) for the update."
                : $"Entity found {typeof(T).Name}(id={id})");
            if (existingEntity == null) return null;

            ctx.Entry(existingEntity).CurrentValues.SetValues(entity);
            var entityEntry = ctx.Update(existingEntity);
            Console.WriteLine($"Entity {typeof(T).Name} {entityEntry.State} ");
            await ctx.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return entityEntry.Entity;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the entity.");
            Debug.WriteLine(ex);
            return null;
        }
    }

    public async Task<bool> DeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            ctx.Remove(entity);
            return await ctx.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting the entity.");
            Debug.WriteLine(ex);
            return false;
        }
    }

    public async Task<bool> ExistsAsync<T>(int id, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var exists = await ctx.Set<T>().FindAsync(id).ConfigureAwait(false) != null;
            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while checking if the entity exists.");
            Debug.WriteLine(ex);
            return false;
        }
    }
}