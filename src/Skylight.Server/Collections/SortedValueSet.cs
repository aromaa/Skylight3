using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Skylight.Server.Collections;

internal sealed class SortedValueSet<TKey, TValue>(IComparer<TKey> keyComparer, IComparer<TValue> valueComparer)
	where TKey : notnull
{
	private readonly Dictionary<TKey, TValue> values = [];

	private ImmutableSortedSet<(TValue Value, TKey Key)> set = ImmutableSortedSet.Create<(TValue, TKey)>(new Comparer(keyComparer, valueComparer));

	public int Size => this.set.Count;

	public TValue Min => this.set.Min.Value;

	public ImmutableSortedSet<(TValue Value, TKey Key)> Values => this.set;

	public void UpdateAndTrim(TKey key, TValue value, int trimSize)
	{
		ImmutableSortedSet<(TValue Value, TKey Key)>.Builder builder = this.set.ToBuilder();

		ref TValue? current = ref CollectionsMarshal.GetValueRefOrAddDefault(this.values, key, out bool exists);
		if (exists)
		{
			builder.Remove((current!, key));
		}
		else
		{
			while (trimSize <= builder.Count)
			{
				(TValue Value, TKey Key) min = builder.Min;

				builder.Remove(min);

				this.values.Remove(min.Key);
			}
		}

		current = value;

		builder.Add((value, key));

		this.set = builder.ToImmutable();
	}

	private sealed class Comparer(IComparer<TKey> keyComparer, IComparer<TValue> valueComparer) : IComparer<(TValue Value, TKey Key)>
	{
		private readonly IComparer<TKey> keyComparer = keyComparer;
		private readonly IComparer<TValue> valueComparer = valueComparer;

		public int Compare((TValue Value, TKey Key) x, (TValue Value, TKey Key) y)
		{
			int result = this.valueComparer.Compare(x.Value, y.Value);

			return result == 0
				? this.keyComparer.Compare(x.Key, y.Key)
				: result;
		}
	}
}
