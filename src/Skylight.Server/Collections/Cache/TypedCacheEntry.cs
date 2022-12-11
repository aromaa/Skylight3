namespace Skylight.Server.Collections.Cache;

internal sealed class TypedCacheEntry<T>
{
	internal T Value { get; }

	internal TypedCacheEntry(T value)
	{
		this.Value = value;
	}
}
