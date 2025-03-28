namespace Ifx.Filtering.Contracts;

public interface IFilteringStrategy<T> where T : class
{
    IQueryable<T> ApplyFiltering(IQueryable<T> query, Filter<T> filter);
}