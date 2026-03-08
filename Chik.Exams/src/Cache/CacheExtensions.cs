
using ZiggyCreatures.Caching.Fusion;

namespace Chik.Exams;

public static class CacheExtensions
{
    /// <summary>
    /// Debounces a cache key, so that it is only set if it has not been set in the last expiration time.
    /// If the key has not been set, it will be set and true will be returned.
    /// If the key has been set, false will be returned.
    /// </summary>
    /// <param name="cache">The cache to debounce.</param>
    /// <param name="key">The key to debounce.</param>
    /// <param name="expiration">The expiration time.</param>
    /// <returns>True if the key was set, false if it was not set.</returns>
    public static bool TryDebounce(
        this IFusionCache cache,
        string key,
        TimeSpan expiration
    )
    {
        var value = cache.TryGet<bool>(key);
        if (!value.GetValueOrDefault())
        {
            cache.Set(key, true, expiration);
            return true;
        }
        return false;
    }
}