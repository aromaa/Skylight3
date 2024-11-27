using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Skylight.API.Collections.Cache;

namespace Skylight.Server.Collections.Cache;

internal sealed class AsyncCache<TKey, TValue>
	where TKey : notnull
	where TValue : class //Making this work for value types is much more work
{
	private readonly ConcurrentDictionary<TKey, AsyncCacheEntry<object?>> cache;

	private readonly Func<TKey, Task<TValue?>> loader;

	internal AsyncCache(Func<TKey, Task<TValue?>> loader)
	{
		this.cache = new ConcurrentDictionary<TKey, AsyncCacheEntry<object?>>();

		this.loader = loader;
	}

	internal ValueTask<TValue?> GetAsync(TKey key)
	{
		AsyncCacheEntry<object?> entry = this.cache.GetOrAdd(key, static _ => new AsyncCacheEntry<object?>(new PendingAsyncTask()));

		object? value = entry.Value;
		if (value is null || value.GetType() != typeof(PendingAsyncTask))
		{
			return ValueTask.FromResult(Unsafe.As<TValue>(value));
		}

		return new ValueTask<TValue?>(Unsafe.As<PendingAsyncTask>(value).LoadAsync(entry, key, this.loader));
	}

	internal ValueTask<ICacheReference<TValue>?> GetValueAsync(TKey key)
	{
		AsyncCacheEntry<object?> entry = this.cache.GetOrAdd(key, static _ => new AsyncCacheEntry<object?>(new PendingAsyncTask()));

		object? value = entry.Value;
		if (value is not null)
		{
			if (value.GetType() != typeof(PendingAsyncTask))
			{
				return ValueTask.FromResult((ICacheReference<TValue>)entry.GetRef<TValue>())!;
			}
		}
		else if (value is null)
		{
			return ValueTask.FromResult<ICacheReference<TValue>?>(null);
		}

		//TODO: Optimize
		return new ValueTask<ICacheReference<TValue>?>(Unsafe.As<PendingAsyncTask>(value).LoadAsync(entry, key, this.loader).ContinueWith(Unwrap, entry));

		static ICacheReference<TValue>? Unwrap(Task<TValue?> task, object? state)
		{
			return task.Result is null
				? null
				: ((AsyncCacheEntry<object?>)state!).GetRef<TValue>();
		}
	}

	private sealed class PendingAsyncTask : TaskCompletionSource<TValue?>
	{
		private volatile bool initialized;

		internal Task<TValue?> LoadAsync(AsyncCacheEntry<object?> entry, TKey key, Func<TKey, Task<TValue?>> loader)
		{
			if (this.initialized || Interlocked.CompareExchange(ref this.initialized, true, false))
			{
				return this.Task;
			}

			loader(key).ContinueWith(static (task, state) =>
			{
				(PendingAsyncTask pendingAsyncTask, AsyncCacheEntry<object?> entry) = ((PendingAsyncTask, AsyncCacheEntry<object?>))state!;
				if (task.IsCompletedSuccessfully)
				{
					//Set successful value first to make it be seen before the events are fired
					entry.Value = task.Result;

					pendingAsyncTask.SetResult(task.Result);
				}
				else
				{
					//Uh oh, we failed. This might be connection issues or some unrelated temporary nonrecurring issue.
					//Do not cache the failed attempt and try to load it again on next request.
					//Set first before the events are fired so exception handling logic can immediately start a new request.
					entry.Value = new PendingAsyncTask();

					if (task.IsFaulted)
					{
						pendingAsyncTask.SetException(task.Exception);
					}
					else
					{
						pendingAsyncTask.SetCanceled();
					}
				}
			}, (This: this, Entry: entry), TaskContinuationOptions.ExecuteSynchronously);

			return this.Task;
		}
	}
}
