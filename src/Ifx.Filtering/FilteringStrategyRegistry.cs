using System.Collections.Concurrent;
using System.Diagnostics;
using Ifx.Filtering.Contracts;
using Ifx.Filtering.Linq;

namespace Ifx.Filtering;

public static class FilteringStrategyRegistry
{
    private static readonly ConcurrentDictionary<string, IFilteringStrategy<object>> Strategies = new();

    static FilteringStrategyRegistry()
    {
        Register("default", new LinqFilteringStrategy<object>());
    }

    public static void Register<T>(string key, IFilteringStrategy<T> strategy) where T : class
    {
        Strategies[key] = (IFilteringStrategy<object>)strategy;
    }

    public static IFilteringStrategy<T>? Get<T>(string key) where T : class
    {
        if (Strategies.TryGetValue(key, out var strategy)) return strategy as IFilteringStrategy<T>;

        Debug.WriteLine($"Key '{key}' not found in FilteringStrategyRegistry.");
        return null;
    }

    public static bool TryGetValue<T>(string key, out IFilteringStrategy<T>? strategy) where T : class
    {
        strategy = null;
        if (Strategies.TryGetValue(key, out var rawStrategy) && rawStrategy is IFilteringStrategy<T> typedStrategy)
        {
            strategy = typedStrategy;
            return true;
        }

        Debug.WriteLine($"Key '{key}' not found or incompatible in FilteringStrategyRegistry.");
        return false;
    }

    public static IFilteringStrategy<T> GetDefault<T>() where T : class
    {
        return Get<T>("default") ?? throw new InvalidOperationException("Default filtering strategy not registered.");
    }
}