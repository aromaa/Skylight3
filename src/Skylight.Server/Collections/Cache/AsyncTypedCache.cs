using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Skylight.Server.Collections.Cache;

internal sealed class AsyncTypedCache<TKey, TValue>
	where TKey : notnull
	where TValue : class? //Making this work for value types is much more work
{
	private readonly ConcurrentDictionary<TKey, TypedCacheEntry<object?>> cache;

	private readonly Func<TKey, Task<TValue>> loader;

	internal AsyncTypedCache(Func<TKey, Task<TValue>> loader)
	{
		this.cache = new ConcurrentDictionary<TKey, TypedCacheEntry<object?>>();

		this.loader = loader;
	}

	internal ValueTask<TValue> GetAsync(TKey key)
	{
		TypedCacheEntry<object?> entry = this.cache.GetOrAdd(key, static _ => new TypedCacheEntry<object?>(new PendingAsyncTask()));

		object? value = entry.Value;
		if (value is null || value.GetType() != typeof(PendingAsyncTask))
		{
			return ValueTask.FromResult(Unsafe.As<TValue>(value!));
		}

		return new ValueTask<TValue>(Unsafe.As<PendingAsyncTask>(value).LoadAsync(entry, key, this.loader));
	}

	private sealed class PendingAsyncTask : TaskCompletionSource<TValue>
	{
		private int initialized;

		internal Task<TValue> LoadAsync(TypedCacheEntry<object?> entry, TKey key, Func<TKey, Task<TValue>> loader)
		{
			if (this.initialized != 0 || Interlocked.CompareExchange(ref this.initialized, 1, 0) == 1)
			{
				return this.Task;
			}

			loader(key).ContinueWith(static (task, state) =>
			{
				(PendingAsyncTask pendingAsyncTask, TypedCacheEntry<object?> entry) = ((PendingAsyncTask, TypedCacheEntry<object?>))state!;
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
