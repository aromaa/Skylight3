using System.Diagnostics;
using Skylight.API.Collections.Cache;

namespace Skylight.Server.Collections.Cache;

internal sealed class AsyncCacheEntryReference<T> : ICacheReference<T>
	where T : class?
{
	private AsyncCacheEntry<T>? entry;

	internal AsyncCacheEntryReference(AsyncCacheEntry<T> entry)
	{
		this.entry = entry;
	}

#if DEBUG
	~AsyncCacheEntryReference()
	{
		Debug.Assert(this.entry is null, "Reference was not disposed");
	}
#endif

	public AsyncCacheEntry<T> Entry
	{
		get
		{
			ObjectDisposedException.ThrowIf(this.entry is null, this.entry);

			return this.entry;
		}
	}

	public T Value => this.Entry.Value;

	public ICacheReference<T> Retain()
	{
		ObjectDisposedException.ThrowIf(this.entry is null, this.entry);

		return this.entry.GetRef();
	}

	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(this.entry is null, this.entry);

		this.entry.ReturnRef();
		this.entry = null;
	}
}
