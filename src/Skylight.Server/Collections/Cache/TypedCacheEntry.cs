namespace Skylight.Server.Collections.Cache;

internal sealed class TypedCacheEntry<T>
{
	internal T Value { get; set; }

	private int count;

	private DateTime lastAccess;

	internal TypedCacheEntry(T value)
	{
		this.Value = value;
	}

	internal TypedCacheEntryReference<T> GetRef()
	{
		Interlocked.Increment(ref this.count);

		return new TypedCacheEntryReference<T>(this);
	}

	internal void ReturnRef()
	{
		this.lastAccess = DateTime.UtcNow;

		Interlocked.Decrement(ref this.count);
	}
}
