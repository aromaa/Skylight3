namespace Skylight.Server.Collections.Cache;

internal struct TypedCacheEntryReference<T> : IDisposable
{
	private TypedCacheEntry<T>? entry;

	internal TypedCacheEntryReference(TypedCacheEntry<T> entry)
	{
		this.entry = entry;
	}

	public readonly TypedCacheEntry<T> Entry
	{
		get
		{
			ObjectDisposedException.ThrowIf(this.entry is null, this.entry);

			return this.entry;
		}
	}

	public readonly T Value => this.Entry.Value;

	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(this.entry is null, this.entry);

		this.entry.ReturnRef();
		this.entry = null;
	}
}
