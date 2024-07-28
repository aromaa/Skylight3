using System.Runtime.CompilerServices;

namespace Skylight.Server.Collections.Cache;

internal sealed class AsyncCacheEntry<T>
	where T : class?
{
	internal T Value { get; set; }

	private int count;

	private DateTime lastAccess;

	internal AsyncCacheEntry(T value)
	{
		this.Value = value;
	}

	internal AsyncCacheEntryReference<T> GetRef()
	{
		Interlocked.Increment(ref this.count);

		return new AsyncCacheEntryReference<T>(this);
	}

	internal AsyncCacheEntryReference<TOther> GetRef<TOther>()
		where TOther : class?
	{
		Interlocked.Increment(ref this.count);

		return new AsyncCacheEntryReference<TOther>(Unsafe.As<AsyncCacheEntry<TOther>>(this));
	}

	internal void ReturnRef()
	{
		this.lastAccess = DateTime.UtcNow;

		Interlocked.Decrement(ref this.count);
	}
}
