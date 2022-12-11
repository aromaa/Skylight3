using System.Diagnostics;
using System.Numerics;

namespace Skylight.Server.Collections;

internal sealed class RangeMap<TKey, TValue>
	where TKey : INumber<TKey>, IComparisonOperators<TKey, TKey, bool>, IMinMaxValue<TKey>
{
	private readonly IComparer<TValue> valueComparer;

	private readonly TreeMap<Slice, SortedSet<TValue>> reservedSlices;
	private readonly SortedSet<Slice> slices;

	internal RangeMap(IComparer<TValue> valueComparer)
	{
		this.valueComparer = valueComparer;

		this.slices = new SortedSet<Slice>(SliceComparer.Instance)
		{
			new(TKey.Zero, TKey.MaxValue)
		};

		this.reservedSlices = new TreeMap<Slice, SortedSet<TValue>>(SliceComparer.Instance);
	}

	internal TValue? Max => this.reservedSlices.Max.Value is { } value ? value.Max : default;

	internal void Add(TKey min, TKey max, TValue value)
	{
		List<Slice> slicesToRemove = new();
		List<Slice> slicesToAdd = new();
		foreach (Slice freeSlice in this.slices.GetViewBetween(new Slice(min, min), new Slice(max, max)))
		{
			slicesToRemove.Add(freeSlice);

			//TODO: These should modify the struct directly
			if (min == freeSlice.Min)
			{
				slicesToAdd.Add(new Slice(max, freeSlice.Max));
			}
			else if (max == freeSlice.Max)
			{
				slicesToRemove.Add(freeSlice);
				slicesToAdd.Add(new Slice(min, freeSlice.Max));
			}
			else
			{
				slicesToAdd.Add(new Slice(freeSlice.Min, min));
				slicesToAdd.Add(new Slice(max, freeSlice.Max));
			}
		}

		foreach (Slice sliceToRemove in slicesToRemove)
		{
			this.slices.Remove(sliceToRemove);
		}

		foreach (Slice sliceToAdd in slicesToAdd)
		{
			this.slices.Add(sliceToAdd);
		}

		Slice slice = new(min, max);

		if (!this.reservedSlices.TryGetValue(slice, out SortedSet<TValue>? values))
		{
			values = new SortedSet<TValue>(this.valueComparer);

			this.reservedSlices.Add(slice, values);
		}

		values.Add(value);
	}

	internal void Remove(TKey min, TKey max, TValue value)
	{
		Slice slice = new(min, max);

		if (this.reservedSlices.TryGetValue(slice, out SortedSet<TValue>? values))
		{
			values.Remove(value);

			if (values.Count > 0)
			{
				return;
			}

			this.reservedSlices.Remove(slice);

			TKey minCanRestore = min;
			TKey maxCanRestore = max;
			foreach ((Slice reservedSlice, _) in this.reservedSlices.GetViewBetween(new Slice(min, min), new Slice(max, max)))
			{
				if (max >= reservedSlice.Max)
				{
					minCanRestore = TKey.Max(minCanRestore, reservedSlice.Max);
				}
				else
				{
					maxCanRestore = TKey.Min(maxCanRestore, reservedSlice.Min);
				}
			}

			if (minCanRestore >= maxCanRestore)
			{
				return;
			}

			Slice sliceToAdd = new(minCanRestore, maxCanRestore);

			List<Slice> slicesToRemove = new();
			foreach (Slice freeSlice in this.slices.GetViewBetween(new Slice(minCanRestore, minCanRestore), new Slice(maxCanRestore, maxCanRestore)))
			{
				slicesToRemove.Add(freeSlice);

				if (sliceToAdd.Max < freeSlice.Max)
				{
					sliceToAdd = new Slice(sliceToAdd.Min, freeSlice.Max);
				}
				else if (sliceToAdd.Min > freeSlice.Min)
				{
					sliceToAdd = new Slice(freeSlice.Min, sliceToAdd.Max);
				}
			}

			foreach (Slice sliceToRemove in slicesToRemove)
			{
				this.slices.Remove(sliceToRemove);
			}

			this.slices.Add(sliceToAdd);
		}
		else
		{
			Debug.Fail("Failed to find slice");
		}

		if (this.reservedSlices.Count == 0)
		{
			Debug.Assert(this.slices.FirstOrDefault() is { } debugSlice && debugSlice.Min == TKey.Zero && debugSlice.Max == TKey.MaxValue);
		}
	}

	internal SortedSet<TValue>? FindNearestValues(TKey target, TKey range, TKey emptySpaceAmount)
	{
		SortedSet<TValue>? values = null;

		TKey distance = TKey.MaxValue;
		foreach (Slice slice in this.slices.GetViewBetween(new Slice(target - range, target - range), new Slice(target + range, target + range)))
		{
			if (emptySpaceAmount > slice.Max - slice.Min)
			{
				continue;
			}

			TKey currentDistance = TKey.Abs(slice.Min - target);
			if (currentDistance > distance)
			{
				continue;
			}

			if (!this.reservedSlices.TryGetValue(new Slice(slice.Min, slice.Min), out values))
			{
				continue;
			}

			distance = currentDistance;
		}

		return values;
	}

	internal readonly struct Slice
	{
		internal TKey Min { get; init; }
		internal TKey Max { get; init; }

		internal Slice(TKey min, TKey max)
		{
			this.Min = min;
			this.Max = max;
		}
	}

	internal sealed class SliceComparer : IComparer<Slice>
	{
		internal static readonly SliceComparer Instance = new();

		public int Compare(Slice x, Slice y)
		{
			if (y.Max >= x.Max && y.Min <= x.Min)
			{
				return 0;
			}

			int result = x.Max.CompareTo(y.Max);
			if (result != 0)
			{
				return result;
			}

			return x.Min.CompareTo(y.Min);
		}
	}
}
