namespace Skylight.Server.Collections.Immutable;

internal readonly struct ImmutableArray2D<T>
{
	internal static readonly ImmutableArray2D<T> Empty = new(new T[0, 0]);

	private readonly T[,]? array;

	private ImmutableArray2D(T[,]? array)
	{
		this.array = array;
	}

	public T this[int x, int y] => this.array![x, y];

	internal sealed class Builder
	{
		private T[,] array;

		internal Builder(int width, int height)
		{
			this.array = new T[width, height];
		}

		public T this[int x, int y]
		{
			get => this.array[x, y];
			set => this.array[x, y] = value;
		}

		public ImmutableArray2D<T> MoveToImmutable()
		{
			(T[,] array, this.array) = (this.array, ImmutableArray2D<T>.Empty.array!);

			return new ImmutableArray2D<T>(array);
		}
	}
}
