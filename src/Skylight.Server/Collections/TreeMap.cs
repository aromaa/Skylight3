using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Skylight.Server.Collections;

internal sealed class TreeMap<TKey, TValue>
{
	private readonly SortedSet<(TKey Key, TValue Value)> backer;

	internal TreeMap(IComparer<TKey>? keyComparer = null)
	{
		this.backer = new SortedSet<(TKey, TValue)>(new KeyComparer(keyComparer));
	}

	internal int Count => this.backer.Count;

	internal (TKey Key, TValue Value) Max => this.backer.Max;

	internal bool Add(TKey key, TValue value)
	{
		return this.backer.Add((key, value));
	}

	internal bool Remove(TKey key)
	{
		return this.backer.Remove((key, default!));
	}

	internal bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
	{
		if (this.backer.TryGetValue((key, default!), out (TKey Key, TValue Value) valueTuple))
		{
			value = valueTuple.Value;

			return true;
		}

		Unsafe.SkipInit(out value);

		return false;
	}

	internal SortedSet<(TKey Key, TValue Value)> GetViewBetween(TKey min, TKey max)
	{
		return this.backer.GetViewBetween((min, default!), (max, default!));
	}

	private sealed class KeyComparer : IComparer<(TKey Key, TValue Value)>
	{
		private readonly IComparer<TKey>? keyComparer;

		internal KeyComparer(IComparer<TKey>? keyComparer)
		{
			this.keyComparer = keyComparer;
		}

		public int Compare((TKey Key, TValue Value) x, (TKey Key, TValue Value) y)
		{
			if (this.keyComparer is not null)
			{
				return this.keyComparer.Compare(x.Key, y.Key);
			}

			return Comparer<TKey>.Default.Compare(x.Key, y.Key);
		}
	}
}
