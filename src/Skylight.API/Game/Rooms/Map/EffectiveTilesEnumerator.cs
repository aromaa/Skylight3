using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Skylight.API.Numerics;

namespace Skylight.API.Game.Rooms.Map;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public struct EffectiveTilesEnumerator(ImmutableArray<Point2D> array, int rotation)
{
	private readonly ImmutableArray<Point2D> array = array;
	private readonly bool swap = (rotation % 8) is 2 or 6;

	private int index = -1;

	public readonly Point2D Current
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => !this.swap
			? this.array[this.index]
			: this.array[this.index].Swap();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool MoveNext()
	{
		return ++this.index < this.array.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly EffectiveTilesEnumerator GetEnumerator() => this;
}
