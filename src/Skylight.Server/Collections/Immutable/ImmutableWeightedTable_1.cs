using System.Collections.Immutable;

namespace Skylight.Server.Collections.Immutable;

internal sealed class ImmutableWeightedTable<TKey>
	where TKey : notnull
{
	internal static readonly ImmutableWeightedTable<TKey> Empty = new(ImmutableArray<Entry>.Empty);

	private readonly ImmutableArray<Entry> entries;

	private ImmutableWeightedTable(ImmutableArray<Entry> entries)
	{
		this.entries = entries;
	}

	internal TKey? Next()
	{
		if (this.entries.IsEmpty)
		{
			return default;
		}

		ref readonly Entry entry = ref this.entries.ItemRef(Random.Shared.Next(this.entries.Length));

		return Random.Shared.NextDouble() < entry.Probability
			? entry.Key
			: entry.Alias;
	}

	private struct Entry
	{
		internal TKey Key { get; }
		internal TKey Alias { get; set; }

		internal double Probability { get; set; }

		internal Entry(TKey key, TKey alias, double probability)
		{
			this.Key = key;
			this.Alias = alias;

			this.Probability = probability;
		}
	}

	internal sealed class Builder
	{
		private readonly Dictionary<TKey, double> entries;

		internal Builder()
		{
			this.entries = new Dictionary<TKey, double>();
		}

		internal void Add(TKey key, double probability) => this.entries.Add(key, probability);

		internal ImmutableWeightedTable<TKey> ToImmutable()
		{
			if (this.entries.Count <= 0)
			{
				return ImmutableWeightedTable<TKey>.Empty;
			}

			double average = 1.0 / this.entries.Count;

			Entry[] entries = new Entry[this.entries.Count];

			Stack<int> small = new();
			Stack<int> large = new();

			int i = 0;
			foreach ((TKey key, double probability) in this.entries)
			{
				if (probability >= average)
				{
					large.Push(i);
				}
				else
				{
					small.Push(i);
				}

				entries[i++] = new Entry(key, key, probability);
			}

			while (small.Count > 0 && large.Count > 0)
			{
				int lessIndex = small.Pop();
				int moreIndex = large.Pop();

				ref Entry less = ref entries[lessIndex];
				ref Entry more = ref entries[moreIndex];

				less.Probability = this.entries[less.Key] * this.entries.Count;
				less.Alias = more.Key;

				double newProbability = this.entries[more.Key] = (this.entries[more.Key] + this.entries[less.Key]) - average;

				if (newProbability >= average)
				{
					large.Push(moreIndex);
				}
				else
				{
					small.Push(moreIndex);
				}
			}

			return new ImmutableWeightedTable<TKey>(ImmutableArray.Create(entries));
		}
	}
}
