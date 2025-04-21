namespace vc.Ifx.Data.Helpers;

public static class RepositoryHelper
{
    public static async Task<IEnumerable<TDto>> GetAllAsync<TDto, TEntity>(Func<Task<IEnumerable<TEntity>>> getEntities, Func<TEntity, TDto> convert)
    {
        var entities = await getEntities();
        return entities.Select(convert);
    }

    public static async Task<TDto?> GetAsync<TDto, TEntity>(int id, Func<int, Task<TEntity?>> getEntity, Func<TEntity, TDto> convert)
    {
        var entity = await getEntity(id);
        return entity == null ? default : convert(entity);
    }

    public static async Task<TDto?> AddAsync<TDto, TEntity>(TDto dto, Func<TDto, TEntity> convert, Func<TEntity, Task<TEntity?>> addEntity, Func<TEntity, TDto> convertBack)
    {
        var entity = convert(dto);
        var addedEntity = await addEntity(entity);
        return addedEntity == null ? default : convertBack(addedEntity);
    }

    public static async Task<TDto?> UpdateAsync<TDto, TEntity>(TDto dto, Func<TDto, TEntity> convert, Func<TEntity, Task<TEntity?>> updateEntity, Func<TEntity, TDto> convertBack)
    {
        var entity = convert(dto);
        var updatedEntity = await updateEntity(entity);
        return updatedEntity == null ? default : convertBack(updatedEntity);
    }

    public static async Task<bool> DeleteAsync<TDto, TEntity>(TDto dto, Func<TDto, TEntity> convert, Func<TEntity, Task<bool>> deleteEntity)
    {
        var entity = convert(dto);
        return await deleteEntity(entity);
    }

    public static async Task<bool> ExistsAsync(Func<int, Task<bool>> existsEntity, int id)
    {
        return await existsEntity(id);
    }
}