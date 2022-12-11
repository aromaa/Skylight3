using System.Collections.Concurrent;

namespace Skylight.Server.Collections.Cache;

//TODO: I'm feeling like making reference counted cache but lets see what we end up with :)
internal sealed class TypedCache<TKey, TValue>
	where TKey : notnull
{
	private readonly ConcurrentDictionary<TKey, TypedCacheEntry<TValue>> cache;

	internal TypedCache()
	{
		this.cache = new ConcurrentDictionary<TKey, TypedCacheEntry<TValue>>();
	}

	internal TypedCacheEntry<TValue> GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TypedCacheEntry<TValue>> value, TArg argument)
	{
		return this.cache.GetOrAdd(key, value, argument);
	}
}
